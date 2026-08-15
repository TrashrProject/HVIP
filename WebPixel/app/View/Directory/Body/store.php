<?php
/**
 * PixelZone by RDP Services, Emulated by Retro Development Server.
 * The use of this program is restricted to clients and owners of RDP Services.
 * Any unauthorized use of this code it'll end up on deletion of the program.
 * Developers P3x & Jeihden.
 * Copyrights © 2020
 * Last Modified: $file.lastModefied
 */
?>
<?php $StoreMG->GetStoreHoverHidden(); ?>
<div class="content">
    <div class="container">
        <div class="row">
            <!-- Sidebar-->
            <div class="col-3">
                <div class="nav flex-column nav-pills" id="store-tab" role="tablist" aria-orientation="vertical">
                    <a style="background-image: url('<?php echo DY; ?>/img/store/crates-gold.png');" class="nav-link active" id="store-keys-tab" data-toggle="pill" href="#store-keys" role="tab" aria-selected="false">
                        <span class="store-item-name">Tienda de Platinos</span>
                    </a>
                    <a style="background-image: url('<?php echo DY; ?>/img/store/vip.png');" class="nav-link" id="store-keys-tab" data-toggle="pill" href="#store-vip" role="tab" aria-selected="false">
                        <span class="store-item-name">Comprar VIP</span>
                    </a>
                </div>

                <div id="GameStats" class="content-box blue leaderboards mb-3">
                    <div class="title">
                        <i class="fas fa-id-card text-secondary"></i> Estad&iacute;sticas de Juego
                    </div>
                    <div class="box-content">
                        <div class="user-info-pz">
                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center">
                                        <div><div class="user-pz-dinero"></div></div>
                                    </div>
                                    <div class="post-info mr-auto ml-2">
                                        <b>Cartera:</b> $<?php echo number_format($UData['credits']); ?>
                                    </div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center">
                                        <div><div class="user-pz-banco"></div></div>
                                    </div>
                                    <div class="post-info mr-auto ml-2">
                                        <b>Banco:</b> $<?php echo number_format($UPData['bank']); ?>
                                    </div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center">
                                        <div><div class="user-pz-platinos"></div></div>
                                    </div>
                                    <div class="post-info mr-auto ml-2">
                                        <b>Platinos:</b> <?php echo $UData['vip_points']; ?>
                                    </div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                            <div class="user-pz-stats">
                                <div class="d-flex justify-content-center align-items-center">
                                    <div class="author-avatar text-center">
                                        <div><div class="user-pz-level"></div></div>
                                    </div>
                                    <div class="post-info mr-auto ml-2">
                                        <b>Nivel:</b> <?php echo $UPData['level']; ?>
                                    </div>
                                    <div class="ml-2 mr-2"><i class="fas fa-certificate"></i></div>
                                </div>
                            </div>

                        </div>
                    </div>
                </div>

                <div class="content-box blue leaderboards mb-3">
                    <div class="title">Más Platinos</div>
                    <div class="box-content">
                        <?php
                        $R_ = $UserMG->GetLeaderBoardPL();
                        while($Row = mysqli_fetch_assoc($R_)): ?>
                            <a class="leaderboard-user no-link-styling justify-content-center align-items-center" href="<?php echo Config::$URL; ?>/profile/<?php echo $Row['id']; ?>" style="<?php echo ($Row['online'] == '1')? "border-top: 3px solid #1dc40e;" : "" ; ?>">
                                <div class="leaderboard-pixel">
                                    <img src="https://nitro-imager.kubbo.ch/?figure=<?php echo $Row['look']; ?>&amp;direction=3" <?php echo ($Row['online'] == '1')?  '' : 'style="filter: grayscale(100%);"' ; ?>>
                                </div>
                                <div class="leaderboard-user-info mr-auto">
                                    <div class="leaderboard-user-name font-weight-bold"><?php echo $Row['username']; ?></div>
                                    <div class="leaderboard-user-stat">

                                        <?php echo number_format($Row['vip_points']); ?> PL

                                    </div>
                                </div>
                            </a>
                        <?php endwhile; ?>
                    </div>
                </div>
            </div>
            <!-- Main Column -->
            <div class="col-9 pl-0 mb-2">
                <div class="tab-content mb-2" style="background: none;box-shadow: none;">

                    <div class="tab-pane p-2 active show" id="store-keys" role="tabpanel">
                        <div id="purchase_loading" style="display: none;">
                            <div id="fb-message" class="alert alert-info">Por favor, espera unos segundos mientras procesamos tu transacción...</div>
                        </div>
                        <div id="purchase_result" >

                        </div>
                        <?php if($UData['online'] >= 1): ?>
                        <div id="fb-message" class="alert alert-danger">Por favor desconectate del juego para poder hacer cualquier tipo de compra, si te conectas mientras la compra está siendo procesada, perderás lo que compraste y el dinero pagado por el mismo.</div>
                        <?php else: ?>

                        <div class="store-grid">
                          <?php $StoreMG->GetStoreItems("pl"); ?>
                        </div>
                        <?php endif; ?>

                        <div class="store-item-info p-2">
                            <div class="content-box">
                                <div class="title">¿Qué son los Platinos?</div>
                                <div class="box-content">
                                    <div class="store-item-info p-2">
                                        <p>Los Platinos de HabboVIP, son la moneda especial que te permitirá hacer compras de objetos especiales dentro del juego. Estas monedas se te otorgan dependiendo la donación que hagas en la tienda. Hay diferentes paquetes de compras que puedes ver en la sección de arriba.
                                        </p>
                                        <p>Los platinos son transferibles dentro del juego. El precio del mismo dependerá del vendedor, sin embargo, es importante saber que el precio promedio actual de 1 PL es: <b>$10,000</b></p>
                                        <p>
                                            <br>
                                            Cosas que puedes comprar con platinos:
                                        </p><ul>
                                            <li> Rares exclusivos sin edición Limitada.</li>
                                            <li> Cajas de almacenamiento de armas.</li>
                                            <li> Membresía VIP1 o VIP2.</li>
                                            <li> Vehículos exóticos.</li>
                                            <li> Mansiones en las avenidas principales, las cuales puedes personalizar.</li>
                                            <li> Floor editor en apartamentos y/o casas.</li>
                                        </ul><br>
                                        ¡Entre otras cosas más mientras se van agregando actualizaciones!
                                        <p></p>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="store-item-info p-2">
                            <div class="content-box">
                                <div class="title">¿Cómo comprar Platinos?</div>
                                <div class="box-content">
                                    <div class="store-item-info p-2">
                                        <b>Es necesario estar desconectado del juego para relizar la transacción.</b>
                                        <br>
                                        <p>Una vez hayas completado el pago del paquete que deseas, el sistema va a verificar la transacción automaticamente con PayPal, y te otorgará los platinos que seleccionaste.</p>
                                        <p>
                                        Ten en cuenta que, si te conectas al juego o sales de la página al realizar un pago y mientras que se te otorgan los platinos, puedes perder los platinos y la donación, a menos que tengas pruebas del pago con el número de transacción. Siempre espera el mensaje de confirmación antes de salir.</p>
                                        <p>
                                        Al hacer la donación, el sistema durará menos de 30 segundos en darte la confirmación y los platinos automaticamente. <br><b>¡Por favor, No refresques ni salgas de la página de la Tienda, hasta que recibas la confirmación del pago!</b></p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="tab-pane p-2" id="store-vip" role="tabpanel">
                        <div id="purchase_loading" style="display: none;">
                            <div id="fb-message" class="alert alert-info">Por favor, espera unos segundos mientras procesamos tu transacción...</div>
                        </div>
                        <div id="purchase_result" >

                        </div>
                        <?php if($UData['online'] >= 1): ?>
                        <div id="fb-message" class="alert alert-danger">Por favor desconectate de el cliente para poder hacer algún tipo de compra, si te conectas mientras la compra está siendo ejecutada, perderás lo que compraste y el dinero pagado por el mismo.</div>
                        <?php else: ?>

                        <div class="store-item-info p-2">
                            <div class="content-box">
                                <div class="title">Comprar VIP</div>
                                <div class="box-content">
                                    <div class="store-item-info p-2">
                                        <style type="text/css">
                                            #customers {
                                              color: black;
                                              border-collapse: collapse;
                                              width: 100%;
                                            }

                                            #customers td, #customers th {
                                              border: 1px solid #ddd;
                                              padding: 8px;
                                            }

                                            #customers tr:nth-child(even){background-color: #f2f2f2;}


                                            #customers th {
                                              padding-top: 12px;
                                              padding-bottom: 12px;
                                              text-align: left;
                                              background-color: #4CAF50;
                                              color: white;
                                            }
                                            .Estilo3 {
                                                color: #666666;
                                                font-weight: bold;
                                            }
                                            .Estilo2 {
                                                color: #FF6600;
                                                font-weight: bold;
                                            }
                                            .Estilo1 {
                                                color: #009900;
                                                font-weight: bold;
                                            }
                                        </style>
                                        <table id="customers">
                                            <tbody>
                                                <tr bgcolor="#f4f4f4">
                                                    <td width="328" height="44" valign="middle">
                                                        <strong>&nbsp;</strong>Beneficio
                                                    </td>
                                                    <td width="115" align="center" valign="middle">
                                                        <span class="Estilo3">Com&uacute;n</span>
                                                    </td>
                                                    <td width="115" align="center" valign="middle">
                                                        <span class="Estilo2">VIP1</span>
                                                    </td>
                                                    <td width="115" align="center" valign="middle" bgcolor="#efefef">
                                                        <span class="Estilo1">VIP2</span>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- L&iacute;mite de veh&iacute;culos propios</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF">2</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF">3</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef">4</td>
                                                </tr>
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FBFBFB">&nbsp;- L&iacute;mite de intereses ganados en el banco</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FBFBFB">$1.000</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FBFBFB">$1.500</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef">$3.000</td>
                                                </tr>
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- L&iacute;mite de propiedades compradas a su nombre</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF">1</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF">1</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef">2</td>
                                                </tr>
                                                <!--<tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- L&iacute;mite de plantas de marihuana</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF">2</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF">2</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef">4</td>
                                                </tr>-->
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- Aumento en ganancia de habilidad trabajando</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<?php echo DY; ?>/img/icons/si.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef"><img src="<?php echo DY; ?>/img/icons/si.png"></td>
                                                </tr>
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FBFBFB">&nbsp;- Tener dos trabajos simultáneamente</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FBFBFB"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FBFBFB"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef"><img src="<?php echo DY; ?>/img/icons/si.png"></td>
                                                </tr>
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- Espacio en el ba&uacute;l/maletero de los veh&iacute;culos</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF">Normal</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF">Normal +1</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef">Normal +2</td>
                                                </tr>
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- Ganancia extra en propinas como mesero en restaurantes</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF">Normal</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF">Normal +10%</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef">Normal +20%</td>
                                                </tr>
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- Límite de cupos extras en Bandas</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF">Normal</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF">Normal +5</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef">Normal +10</td>
                                                </tr>
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FBFBFB">&nbsp;- Reducci&oacute;n de condena en prisi&oacute;n</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FBFBFB"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FBFBFB">10%</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef">25%</td>
                                                </tr>
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- Camiones adicionales para el trabajo de camionero</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef"><img src="<?php echo DY; ?>/img/icons/si.png"></td>
                                                </tr>
                                                <!--<tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- Crear su propia emisora de radio y transmitir en ella</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<\?php echo CDN; ?>/general/me/img/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<\?php echo CDN; ?>/general/me/img/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef"><img src="<\?php echo CDN; ?>/general/me/img/si.png"></td>
                                                </tr>-->
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- Trabajar de basurero sin el uniforme requerido</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef"><img src="<?php echo DY; ?>/img/icons/si.png"></td>
                                                </tr>
                                                <!--<tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- Ganancia adicional trabajando de transportista</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<\?php echo CDN; ?>/general/me/img/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<\?php echo CDN; ?>/general/me/img/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef"><img src="<\?php echo CDN; ?>/general/me/img/si.png"></td>
                                                </tr>-->
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FBFBFB">&nbsp;- Trabajo de ladr&oacute;n sin requisitos previos</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FBFBFB"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FBFBFB"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef"><img src="<?php echo DY; ?>/img/icons/si.png"></td>
                                                </tr>
                                                <!--<tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- Asegurada la pesca de ejemplares de mayor peso</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<\?php echo CDN; ?>/general/me/img/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<\?php echo CDN; ?>/general/me/img/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef"><img src="<\?php echo CDN; ?>/general/me/img/si.png"></td>
                                                </tr>-->
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- Servicio de gr&uacute;a gratis para todos los veh&iacute;culos</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef"><img src="<?php echo DY; ?>/img/icons/si.png"></td>
                                                </tr>
                                                <tr>
                                                    <td width="313" height="30" valign="middle" bgcolor="#FFFFFF">&nbsp;- Reducci&oacute;n del aumento de hambre e higiene</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FFFFFF"><img src="<?php echo DY; ?>/img/icons/no.png"></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef"><img src="<?php echo DY; ?>/img/icons/si.png"></td>
                                                </tr>
                                                <tr>
                                                    <td width="313" height="30" align="right" valign="middle" bgcolor="#FBFBFB">&nbsp;</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FBFBFB">Gratis</td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#FBFBFB"><strong>5 PL</strong> <i>/ mes</i></td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef"><strong>10 PL</strong> <i>/ mes</i></td>
                                                </tr>
                                                <tr bgcolor="#f4f4f4">
                                                    <td width="313" height="44" valign="middle"></td>
                                                    <td width="120" align="center" valign="middle"></td>
                                                    <td width="120" align="center" valign="middle">
                                                        <button name="BuyVIP" type="button" class="button green enter-apex no-link-styling" onclick="return BuyVIP(1)">Comprar</button>
                                                    </td>
                                                    <td width="120" align="center" valign="middle" bgcolor="#efefef">
                                                        <button name="BuyVIP" type="button" class="button green enter-apex no-link-styling" onclick="return BuyVIP(2)">Comprar</button>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <?php endif; ?>

                    </div>


                    <div class="text-center text-white font-weight-bold pb-3"><small>Todas las compras se aplican automaticamente a su cuenta al confirmar el pago. Al usar esta tienda acepta nuestros términos y condiciones de uso, y acepta que ningunas de las compras tienen devolución <b>ATT: Administración de HabboVIP.</b></small></div>
                </div>
            </div>
        </div>
    </div>
</div>

<script type="text/javascript">
    function BuyVIP(VT){
        var data = "vipType=" + VT;

        $.ajax({
            type:'POST',
            url:'/store.php',
            data:data,
            success:function(resp){
                var Data = jQuery.parseJSON(resp);
                Swal.fire(
                  Data["title"],
                  Data["text"],
                  Data["type"]
                );
                $("#GameStats").load(location.href + " #GameStats");
            }
        });
        return false;
    }

</script>