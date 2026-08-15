/*$(function(){
    var myArray = ['/adsense/ad1.php', '/adsense/ad2.php', '/adsense/ad5.php']; 
    var rand = myArray[(Math.random() * myArray.length) | 0];
    var time = $(".head strong");
    var closeSeconds = $("#client-banner").attr("data-close");

    setInterval(function() { $.get(rand,function(data){
        rand = myArray[(Math.random() * myArray.length) | 0];
        console.log(rand);
        setTimeout(function() {
            $('#client-banner').fadeTo(3000, 1, function(){
                $(this).css("visibility", "visible")
            }); // duration, opacity, callback	
            $('.body').fadeTo(3000, 1, function(){
                $(this).css("display", "block")
            }); // duration, opacity, callback		
            $(".body").html(data);
            time.html(closeSeconds);
            var interval = setInterval(function(){
                time.html(closeSeconds);
                closeSeconds--;
                console.log(closeSeconds);
                if(closeSeconds < 0){
                  clearInterval(interval);
                  closeSeconds = $("#client-banner").attr("data-close");
                }
                
              }, 1000)
            setTimeout(function() {
                $('#client-banner').fadeTo(1500, 0, function(){
                    $(this).css("visibility", "hidden")
                });// duration, opacity, callback	
                $('.body').fadeTo(1500, 0, function(){
                    $(this).css("display", "none")
                });// duration, opacity, callback					    
            }, 30000);
        }, 2000);
    }); }, 1500000);

    setTimeout(function() { $.get(rand,function(data){
        rand = myArray[(Math.random() * myArray.length) | 0];
        console.log(rand);
        setTimeout(function() {
            $('#client-banner').fadeTo(3000, 1, function(){
                $(this).css("visibility", "visible")
            }); // duration, opacity, callback		
            $('#client-banner').fadeTo(3000, 1, function(){
                $(this).css("display", "block")
            }); // duration, opacity, callback	
            $(".body").html(data);	
            time.html(closeSeconds);
            var interval = setInterval(function(){
                time.html(closeSeconds);
                closeSeconds--;
                console.log(closeSeconds);
                if(closeSeconds < 0){
                  clearInterval(interval);
                  closeSeconds = $("#client-banner").attr("data-close");
                }
                
              }, 1000)				
            setTimeout(function() {
                $('#client-banner').fadeTo(1500, 0, function(){
                    $(this).css("visibility", "hidden")
                });// duration, opacity, callback	
                $('.body').fadeTo(1500, 0, function(){
                    $(this).css("display", "none")
                });// duration, opacity, callback
            }, 30000);
        }, 2000);
    }); }, 60000);
});*/

$(function(){
    var random = Math.floor(Math.random() * $('.item').length);
    $('.item').hide().eq(random).show();
    var TimesMoved = 0;
    var TimesToBeMove = Math.floor(Math.random() * (6 - 2 + 1)) + 2;

    var myArray2 = ['/adsense/ad3.php', '/adsense/ad4.php', '/adsense/ad6.php']; 
    var rand2 = myArray2[(Math.random() * myArray2.length) | 0];

    setInterval(function() { $.get(rand2,function(data){
        rand2 = myArray2[(Math.random() * myArray2.length) | 0];
        console.log(rand2);
        $('#AdBox2').fadeTo(3000, 1, function(){
            $(this).css("visibility", "visible")
        }); // duration, opacity, callback		
        $('#AdBox').fadeTo(3000, 1, function(){
            $(this).css("display", "block")
        }); // duration, opacity, callback		
        $("#AdBox").html(data);			
    }); }, 1500000);

    setTimeout(function() { $.get(rand2,function(data){
        console.log(rand2);
        $('#AdBox2').fadeTo(3000, 1, function(){
            $(this).css("visibility", "visible")
        }); // duration, opacity, callback	
        $('#AdBox').fadeTo(3000, 1, function(){
            $(this).css("display", "block")
        }); // duration, opacity, callback		
        $("#AdBox").html(data);		
    }); }, 300000); 

    
   if(rand2 == "/adsense/ad3.php" && TimesToBeMove == 6){$("#exitb").on({
        mouseover:function(){
            if(TimesMoved < TimesToBeMove){
                console.log(TimesToBeMove);
                var xd = Math.floor(Math.random() * (800 - 100 + 1) + 500);
                var $this = $(this);

                timer = setTimeout(function () {
                    $this.css({
                        left:(Math.random()*250)+"px",
                        top:(Math.random()*200)+"px",
                    });
                }, xd);

                TimesMoved++;
            }
        }
    });

    $("#exitb").click(function(){
        if(TimesMoved >= TimesToBeMove){
            $('#AdBox2').fadeTo(1500, 0, function(){
                $(this).css("visibility", "hidden")
            });// duration, opacity, callback
            $('#AdBox').fadeTo(1500, 0, function(){
                $(this).css("display", "none")
            });// duration, opacity, callback	
            TimesMoved = 0;
            TimesToBeMove = Math.floor(Math.random() * (6 - 2 + 1)) + 2;
        }
    }); }

    else { $("#exitb").on({
        mouseover:function(){
            if(TimesMoved < TimesToBeMove){
                $(this).css({
                    left:(Math.random()*250)+"px",
                    top:(Math.random()*200)+"px",
                });

                TimesMoved++;
            }
        }
    });

    $("#exitb").click(function(){
        $('#AdBox2').fadeTo(1500, 0, function(){
            $(this).css("visibility", "hidden")
        });// duration, opacity, callback
        $('#AdBox').fadeTo(1500, 0, function(){
            $(this).css("display", "none")
        });// duration, opacity, callback		
        TimesMoved = 0;
        TimesToBeMove = Math.floor(Math.random() * (6 - 2 + 1)) + 2;
        }); }
    });