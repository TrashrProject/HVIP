
<?php



function LOAD_MPU($dir, $is_img = false, $folder = null){
    $URL = "https://swf.habbovip.us//V5-0-2/MPU/pz_v2_hween/";
    $ffs = scandir($dir);

    unset($ffs[array_search('.', $ffs, true)]);
    unset($ffs[array_search('..', $ffs, true)]);

    // prevent empty ordered elements
    if (count($ffs) < 1)
        return;

    if($is_img):

        foreach($ffs as $ff){
            return "<img src='". $URL . $folder. "/". $ff ."' style='display: none;' />";
            echo "<br>";
        }
    else:
        foreach($ffs as $ff){
            $F = $ff;
            if(is_dir($dir.DIRECTORY_SEPARATOR.$ff)):
                echo LOAD_MPU($dir.DIRECTORY_SEPARATOR.$ff, true, $F);
            endif;
        }
    endif;
}

LOAD_MPU(dirname(__FILE__));

?>