
/*
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */

// Register Username FB
$(document).ready(function(){
    $("#submit-fb-username").click(function(){
        var U = $("input[name=\"pz-fb-uname\"]").val();
        var E = $("input[name=\"pz-fb-email\"]").val();
        var ID = $("input[name=\"pz-fb-id\"]").val();
        var N = $("input[name=\"pz-fb-name\"]").val();


        if( U != "" && E != "" && ID != ""){
            $.post("/fb_login?fb_submit",
                {
                    fb_u: '' + U,
                    fb_e: '' + E,
                    fb_id: '' + ID,
                    fb_name: '' + N

                },
                function(data){
                     var res = $.parseJSON(data);
                     if(res.result == true){
                         // Close this windows refresh main windows

                         $("#fb-message" ).show();
                         $("#fb-msg").html(res.msg);

                         setTimeout(function(){
                             window.opener.location.href = "/me?newuser=true";
                             self.close();
                             }, 2000);

                     } else {
                         $("#e-fb-message" ).show();
                         $("#e-fb-msg").html(res.msg);
                     }
                });

        } else {
            $("#e-fb-message" ).show();
            $("#e-fb-msg").html("¡Debes llenar todos los campos!");
        }
    });

    // Verify Username
    $('input[name=\"pz-fb-uname\"]').keyup(function(){
        var U = $("input[name=\"pz-fb-uname\"]").val();
        $.post("/fb_login?fb_reg",
            {
                uname: '' + U
            },
            function(data){
                var res = $.parseJSON(data);
                console.log(U);
                if(res.result == false){
                    $("input[name=\"pz-fb-uname\"]").css({"border-style": "solid", "border-color": "#aa0000", "border-width" : "3px"})
                } else {
                    $("input[name=\"pz-fb-uname\"]").css({"border-style": "solid", "border-color": "#23a305", "border-width" : "3px"})
                }
            });
    });
});