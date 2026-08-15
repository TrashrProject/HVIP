<?php


class StoreManager
{
    // Create DB Variable
    protected $DB;

    public function __construct($DB)
    {
        // Get DB into this class
        $this->DB = $DB;
    }

    // Gets Items and shows them in HTML
    public function GetStoreItems($T){
        // Gets all the store items in DB
        $_ITEMS = $this->DB->query("SELECT * FROM store_items WHERE item_type = '". $T ."' AND enabled = '1'");

        // Generate HTML
        if(mysqli_num_rows($_ITEMS) > 0):
            while ($ITEM = mysqli_fetch_assoc($_ITEMS)):
                echo "<div class=\"store-purchase-banner mr-1 flex-fill\">";
                echo "<div class=\"d-flex flex-column\">";
                echo "<div class=\"title\">". $ITEM['item_name'] ." - <i> US $". $ITEM['item_price'] ."</i></div>";
                echo "<div class=\"purchase-info mb-2\" style=\"padding: 0;\"> <img class=\"store_item_img_". $ITEM['id'] ."\" src=\"". $ITEM['item_image'] .".png?V1\"/> </div>";
                echo "<div class=\"purchase-button\" style=\"margin: 1px 15px;\"><div id=\"paypal-button-container-" . $ITEM['id'] . "\"></div></div>";
                echo "</div>";
                echo "</div>";
            endwhile;
        endif;

    }

    public function GetPaypalJavascript(){

        // Gets all the store items in DB
        $_ITEMS = $this->DB->query("SELECT * FROM store_items WHERE enabled = '1'");

        // Generate HTML
        if(mysqli_num_rows($_ITEMS) > 0):

            echo '<script>';

            while ($ITEM = mysqli_fetch_assoc($_ITEMS)):

                echo 'paypal.Buttons({';
                echo 'style: { color: \'blue\', label: \'pay\', height: 25 },';
                echo 'funding: {  disallowed: [ paypal.FUNDING.VENMO, paypal.FUNDING.ELV, paypal.FUNDING.CREDIT ]  },';
                echo 'createOrder: function(data, actions) {';
                echo 'return actions.order.create({';
                echo 'purchase_units: [{';
                echo 'amount: { value: \''. $ITEM['item_price'] .'\'';
                echo ' }, custom_id: \'' .$ITEM['item_code'] . '\'}]});},';
                echo 'onApprove: function(data, actions) {';
                echo '$(\'#purchase_loading\').fadeIn("slow");';
                echo 'return actions.order.capture().then(function(details) {';
                echo ' $.post("/store/checkout", { \'itemID\' : details.purchase_units[0].custom_id, \'orderID\' : details.id }, function (data) { $(\'#purchase_loading\').fadeOut("slow"); $(\'#purchase_result\').html(data);} );';
                echo '});}';
                echo '}).render(\'#paypal-button-container-'. $ITEM['id'] .'\');';
                echo '';

            endwhile;

            echo '</script>';
        endif;

    }

    public function GetStoreCSS(){

        //Gets all the store items in DB
        $_ITEMS = $this->DB->query("SELECT id, enabled, item_image FROM store_items WHERE enabled = '1'");
        $_ITEMS_TOTAL = mysqli_num_rows($_ITEMS);

        if($_ITEMS_TOTAL > 0):
            echo "<style>";
            $ITEMS_COUNT = 0;
            $ITEM_SHAKE_CSS = "";
            while ($ITEM = mysqli_fetch_assoc($_ITEMS)):
                echo "#paypal-button-container-". $ITEM['id'] .":hover { animation: shake 0.5s; animation-iteration-count: infinite; }";
                echo ".store_item_img_". $ITEM['id'].":hover {content: url('". $ITEM['item_image'] ."hover.png?V1'); transform: scale(1.2);}";
                $ITEMS_COUNT++;
                $ITEM_CSS_COMMA = ($ITEMS_COUNT != $_ITEMS_TOTAL)? ", " : " ";
                $ITEM_SHAKE_CSS .= ".store_item_img_". $ITEM['id'] . $ITEM_CSS_COMMA;
            endwhile;
                echo $ITEM_SHAKE_CSS . "{ transition: transform .5s ease; }";
            echo "</style>";
        endif;

    }

    public function GetStoreHoverHidden(){
        //Gets all the store items in DB
        $_ITEMS = $this->DB->query("SELECT id, enabled, item_image FROM store_items WHERE enabled = '1'");
        $_ITEMS_TOTAL = mysqli_num_rows($_ITEMS);

        if($_ITEMS_TOTAL > 0):
            while ($ITEM = mysqli_fetch_assoc($_ITEMS)):
                echo "<img src='". $ITEM['item_image'] ."hover.png?V1' style='display:none;'/>";
            endwhile;
        endif;

    }




}