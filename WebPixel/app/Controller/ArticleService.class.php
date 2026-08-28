<?php
class ArticleService {
    private mysqli $db;
    public function __construct(mysqli $db){$this->db=$db;}
    private function stmt(string $sql,string $types='',array $params=[]): mysqli_stmt {
        $stmt=$this->db->prepare($sql);if(!$stmt)throw new RuntimeException('Requête articles invalide.');
        if($types!=='')$stmt->bind_param($types,...$params);$stmt->execute();return $stmt;
    }
    public function slug(string $title,int $ignoreId=0):string{
        $base=iconv('UTF-8','ASCII//TRANSLIT//IGNORE',$title)?:$title;$base=strtolower(trim(preg_replace('/[^a-zA-Z0-9]+/','-',$base),'-'))?:'article';$base=substr($base,0,165);$slug=$base;$n=2;
        while($this->findBySlug($slug,true) && (int)$this->findBySlug($slug,true)['id']!==$ignoreId){$slug=substr($base,0,155).'-'.$n++;}return $slug;
    }
    public function sanitize(string $html):string{
        $html=strip_tags($html,'<p><br><strong><b><em><i><h2><h3><ul><ol><li><a><blockquote>');
        $html=preg_replace('/\s+on[a-z]+\s*=\s*("[^"]*"|\'[^\']*\'|[^\s>]+)/i','',$html);
        $html=preg_replace('/\s+(style|class|id)\s*=\s*("[^"]*"|\'[^\']*\'|[^\s>]+)/i','',$html);
        $html=preg_replace_callback('/<a\s+([^>]*)>/i',function($m){if(!preg_match('/href\s*=\s*["\']([^"\']+)["\']/i',$m[1],$h))return '<a>'; $url=trim($h[1]);return preg_match('#^(https?://|/)#i',$url)?'<a href="'.htmlspecialchars($url,ENT_QUOTES,'UTF-8').'" rel="noopener noreferrer">':'<a>';},$html);
        return trim($html);
    }
    public function find(int $id):?array{$s=$this->stmt('SELECT a.*,u.username author_name FROM cms_articles a LEFT JOIN users u ON u.id=a.author_id WHERE a.id=?','i',[$id]);return $s->get_result()->fetch_assoc()?:null;}
    public function findBySlug(string $slug,bool $includeDraft=false):?array{$sql='SELECT a.*,u.username author_name FROM cms_articles a LEFT JOIN users u ON u.id=a.author_id WHERE a.slug=?'.($includeDraft?'':' AND a.published=1 AND a.published_at<=?').' LIMIT 1';$s=$includeDraft?$this->stmt($sql,'s',[$slug]):$this->stmt($sql,'si',[$slug,time()]);return $s->get_result()->fetch_assoc()?:null;}
    public function latest(int $limit=4,int $offset=0):mysqli_result{$limit=max(1,min(20,$limit));$offset=max(0,$offset);return $this->db->query("SELECT a.id,a.title,a.slug,a.summary,a.image_url,a.published_at,u.username author_name FROM cms_articles a LEFT JOIN users u ON u.id=a.author_id WHERE a.published=1 AND a.published_at<=".time()." ORDER BY a.published_at DESC,a.id DESC LIMIT $limit OFFSET $offset");}
    public function adminList(string $search='',string $status='all',string $sort='newest',int $limit=20,int $offset=0):mysqli_result{$where='1';$types='';$params=[];if($search!==''){$where.=' AND (a.title LIKE ? OR a.slug LIKE ?)';$like='%'.$search.'%';$types.='ss';$params=[$like,$like];}if($status==='published')$where.=' AND a.published=1';elseif($status==='draft')$where.=' AND a.published=0';$order=$sort==='oldest'?'a.created_at ASC':'a.created_at DESC';$limit=max(1,min(100,$limit));$offset=max(0,$offset);return $this->stmt("SELECT a.*,u.username author_name FROM cms_articles a LEFT JOIN users u ON u.id=a.author_id WHERE $where ORDER BY $order LIMIT $limit OFFSET $offset",$types,$params)->get_result();}
    public function save(int $id,array $data,int $authorId):int{$now=time();$title=substr(trim((string)$data['title']),0,160);if($title==='')throw new RuntimeException('Le titre est obligatoire.');$content=$this->sanitize((string)$data['content']);if($content==='')throw new RuntimeException('Le contenu est obligatoire.');$summary=substr(trim(strip_tags((string)$data['summary'])),0,300);$published=!empty($data['published'])?1:0;$publishedAt=$published?(int)($data['published_at']?:$now):null;$image=$data['image_url']?:null;$slug=$this->slug($title,$id);
        if($id){$this->stmt('UPDATE cms_articles SET title=?,slug=?,summary=?,content=?,image_url=?,published=?,published_at=?,updated_at=? WHERE id=?','sssssiiii',[$title,$slug,$summary,$content,$image,$published,$publishedAt,$now,$id]);return $id;}
        $this->stmt('INSERT INTO cms_articles(title,slug,summary,content,image_url,author_id,published,published_at,created_at,updated_at) VALUES(?,?,?,?,?,?,?,?,?,?)','sssssiiiii',[$title,$slug,$summary,$content,$image,$authorId,$published,$publishedAt,$now,$now]);return (int)$this->db->insert_id;
    }
    public function delete(int $id):void{$this->stmt('DELETE FROM cms_articles WHERE id=?','i',[$id]);}
    public function upload(array $file,string $root):?string{if(($file['error']??UPLOAD_ERR_NO_FILE)===UPLOAD_ERR_NO_FILE)return null;if(($file['error']??1)!==UPLOAD_ERR_OK)throw new RuntimeException('L’image n’a pas pu être envoyée.');if((int)$file['size']>4*1024*1024)throw new RuntimeException('Image trop lourde (4 Mo maximum).');$mime=(new finfo(FILEINFO_MIME_TYPE))->file($file['tmp_name']);$ext=['image/jpeg'=>'jpg','image/png'=>'png','image/webp'=>'webp','image/gif'=>'gif'][$mime]??null;if(!$ext)throw new RuntimeException('Format autorisé : JPG, PNG, WEBP ou GIF.');$dir=$root.'/Dynamics/uploads/articles';if(!is_dir($dir)&&!mkdir($dir,0755,true))throw new RuntimeException('Dossier d’upload indisponible.');$name=bin2hex(random_bytes(16)).'.'.$ext;if(!move_uploaded_file($file['tmp_name'],$dir.'/'.$name))throw new RuntimeException('Impossible d’enregistrer l’image.');return 'Dynamics/uploads/articles/'.$name;}
}
