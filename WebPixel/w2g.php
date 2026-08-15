<!DOCTYPE html>
<html>
<head>
    <title>W2G</title>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
</head>
<body>
    <button id="W2G" type="submit">Test</button>
    <div id="videoResult"></div>
</body>
<script type="text/javascript">
const btnW2G = document.getElementById('W2G');

btnW2G.addEventListener('click', getW2G);

function getW2G(){
    
    let cabecera = new Headers({
        'Content-Type' : 'application/json',
        'Ocp-Apim-Subscription-Key' : 'dywq403v7mut3405dzvv079u3bfs788mugh0z11qx8iy6ypj11ocm4kf5ba8l7i6'
    });

    const proxyurl = "https://cors-anywhere.herokuapp.com/";
    const url = "https://w2g.tv/rooms/create.json";
    const ReqURL = proxyurl + url;

    let objetoInit = {
        method : 'POST',
        body : JSON.stringify({
            'w2g_api_key' : 'dywq403v7mut3405dzvv079u3bfs788mugh0z11qx8iy6ypj11ocm4kf5ba8l7i6',
            'share' : 'https://www.youtube.com/watch?v=KtbvYnpHDLk',
            'bg_color' : '#00ff00',
            'bg_opacity' : '50'
        }),
        headers : cabecera
    };

    let request = new Request(ReqURL, objetoInit);

    fetch(request).then(response => {
        if(response.ok){
            return response.json();
        }
        else{
            return Promise.reject(new Error(response.statusText));
        }
    }).then(response => {
        var RoomW2G = response["streamkey"];
        $("#videoResult").html('<iframe width="420" height="345" src="https://w2g.tv/rooms/'+RoomW2G+'"></iframe>');

    }).catch(error => {
        console.log(error);
        alert('Ha ocurrido un problema al intentar analizar la imagen. Vuelva a intentarlo o ingrese una diferente.');
    });
}
</script>
</html>