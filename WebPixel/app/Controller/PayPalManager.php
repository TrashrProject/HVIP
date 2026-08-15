<?php


class PayPalManager extends Config
{
    protected $DB;
    protected $OrderID;
    protected $ItemID;
    protected $_ITEM; // Stores the item info
    protected $UData;

    public function __construct($DB, $OrderID, $ItemID, $UData)
    {
        // Get DB into this class
        $this->DB = $DB;
        $this->OrderID = $OrderID;
        $this->ItemID = $ItemID;
        $this->UData = $UData;

    }

    // Executes Transaction
    public function ProcessTransaction() {

        // Validate Transaction
        if(!$this->ValidateTransaction()):
            // Returns Error message
            return $this->GetMessage("<b>[ERR#1001]</b> Esta orden es invalida, si tienes dudas contacta a un administrador en nuestro discord haciendo <a href='". Config::$DiscordInvite ."' target='_blank' > Click Aquí</a>", "¡Oops!", true);
        endif;

        // Get Item Information
        $I = $this->DB->query("SELECT * FROM store_items WHERE item_code = '". $this->ItemID ."' LIMIT 1");

        // Store item info in a variable
        $this->_ITEM = mysqli_fetch_assoc($I);

        // Execute transaction

        // Request Transaction Information from PayPal [POST]
        $PP_TRANS = json_decode($this->GetPPTransactionInfo());
        $CName      = $PP_TRANS->payer->name->given_name . " " . $PP_TRANS->payer->name->surname;
        $CEmail     = $PP_TRANS->payer->email_address;
        $PaymentID  = $PP_TRANS->purchase_units[0]->payments->captures[0]->id;
        $OrderVal   = $PP_TRANS->purchase_units[0]->payments->captures[0]->seller_receivable_breakdown->gross_amount->value;
        $PFee       = $PP_TRANS->purchase_units[0]->payments->captures[0]->seller_receivable_breakdown->paypal_fee->value;
        $OrderNet   = $PP_TRANS->purchase_units[0]->payments->captures[0]->seller_receivable_breakdown->net_amount->value;
        $OStatus    = $PP_TRANS->status;

        // Validate transaction result
        if($OStatus == "COMPLETED"):
            // finish transaction and give up the product
            if($this->UData['online'] == '1'):
                // Send mus to give item VALUE
                // Give item value directly to DB
                switch ($this->_ITEM['item_type']):
                    case "pl":
                        // send mus
                endswitch;

            else:
                // Give item value directly to DB
                switch ($this->_ITEM['item_type']):
                    case "pl":
                        $this->GivePLToUser($this->_ITEM['item_value']);
                endswitch;
            endif;
            $Pomotion = ($this->_ITEM['item_promotional_price'] == 0)? '0' : '1';
            $this->InsertTransaction($this->_ITEM['id'], $this->ItemID, $OrderVal, '1', $Pomotion, $this->OrderID, $PaymentID, $PFee, $OrderNet, $CName, $CEmail, $OStatus);
            return $this->GetMessage("Tu orden ha sido excitosa, se te han entregado <b>". $this->_ITEM['item_name'] ."</b> a su cuenta, Gracias por tu colaboración, numero de orden <b>#[". $this->OrderID ."]</b>", "¡Excelente!", false);

        elseif ($OStatus == "PENDING"):
            // Payment under review
            $Pomotion = ($this->_ITEM['item_promotional_price'] == 0)? '0' : '1';
            $this->InsertTransaction($this->_ITEM['id'], $this->ItemID, $OrderVal, '0', $Pomotion, $this->OrderID, $PaymentID, $PFee, $OrderNet, $CName, $CEmail, $OStatus);
            return $this->GetMessage("<b>[ERR#1003]</b> Tu orden ha sido puesta bajo revision de paypal, por favor haz <a href='". Config::$DiscordInvite ."' target='_blank' >Click Aquí</a> y reporta este mensaje a un administrador, numero de orden <b>#[". $this->OrderID ."]</b>", "¡Oops!", true);

        else:
            // Payment declined or refunded
            $Pomotion = ($this->_ITEM['item_promotional_price'] == 0)? '0' : '1';
            $this->InsertTransaction($this->_ITEM['id'], $this->ItemID, $OrderVal, '0', $Pomotion, $this->OrderID, $PaymentID, $PFee, $OrderNet, $CName, $CEmail, $OStatus);
            return $this->GetMessage("Tu metodo de pago ha sido rechazado por paypal, si crees que es un error por favor haz <a href='". Config::$DiscordInvite ."' target='_blank' >Click Aquí</a> y reporta este mensaje a un administrador, numero de orden <b>#[". $this->OrderID ."]</b>", "¡Oops!", true);
        endif;


        // Print Message

    }

    // Validates the transaction
    public function ValidateTransaction () {

        // Verifies if there is a order with that ID alredy
        $R = $this->DB->query("SELECT null FROM store_transactions WHERE transaction_orderid = '". $this->OrderID."' LIMIT 1");
        if(mysqli_num_rows($R) > 0)
            return false;

        // Verifies if the Item requested exists on the list
        $_R = $this->DB->query("SELECT null FROM store_items WHERE item_code = '". $this->ItemID ."' LIMIT 1");
        if(mysqli_num_rows($_R) <= 0)
            return false;

        // Transaction is valid
        return true;

    }

    // Call Paypal via POST to get one time token
    public function GetPayPalToken() {

        $ch = curl_init();
        $clientId = (Config::$SandBox)? Config::$S_PAYPAL_API : Config::$PAYPAL_API;
        $secret = (Config::$SandBox)? Config::$S_PAYPAL_SECRET : Config::$PAYPAL_SECRET ;
        $url = (Config::$SandBox)? "https://api.sandbox.paypal.com/v1/oauth2/token" : "https://api.paypal.com/v1/oauth2/token" ;

        curl_setopt($ch, CURLOPT_URL, $url);
        curl_setopt($ch, CURLOPT_HEADER, false);
        curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
        curl_setopt($ch, CURLOPT_SSLVERSION , 6); //NEW ADDITION
        curl_setopt($ch, CURLOPT_POST, true);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($ch, CURLOPT_USERPWD, $clientId.":".$secret);
        curl_setopt($ch, CURLOPT_POSTFIELDS, "grant_type=client_credentials");

        $result = curl_exec($ch);

        if(empty($result))die("Error: No response.");
        else
        {
            $json = json_decode($result);
            return $json->access_token;
        }

        curl_close($ch); //THIS CODE IS NOW WORKING!

    }

    // Get Paypal transaction info by POST request
    public function GetPPTransactionInfo(){

        $url = (Config::$SandBox)? "https://api.sandbox.paypal.com/v2/checkout/orders/" : "https://api.paypal.com/v2/checkout/orders/" ;
        $GET = $url . $this->OrderID;
        $PP_TOKEN = $this->GetPayPalToken();
        $curl = curl_init($GET);
        curl_setopt($curl, CURLOPT_POST, false);
        curl_setopt($curl, CURLOPT_SSL_VERIFYPEER, false);
        curl_setopt($curl, CURLOPT_HEADER, false);
        curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($curl, CURLOPT_HTTPHEADER, array(
            'Authorization: Bearer ' . $PP_TOKEN,
            'Accept: application/json',
            'Content-Type: application/json'
        ));
        $response = curl_exec($curl);

        $json = $response;
        return $json;
    }


    // Inserts Transaction in DataBase
    public function InsertTransaction($ItemID, $ItemCode, $ItemPrice, $ItemProvided, $ItemPromotion, $OrderID, $PaymentID, $PaypalFee, $PaypalNet, $PaypalCustomerName, $PaypalCustomerEmail, $TransactionStatus) {

        // INSERT CONSULT
        $this->DB->query("INSERT INTO store_transactions (item_id, item_code, item_price, item_provided, item_promotion, transaction_orderid, transaction_paymentid, transaction_paypal_fee, transaction_net, transaction_userid, transaction_payers_name, transaction_payers_email, transaction_time, transaction_status) VALUES (". $ItemID .", '". $ItemCode ."', ". $ItemPrice .", '". $ItemProvided ."', '". $ItemPromotion ."', '". $OrderID ."', '". $PaymentID ."', ". $PaypalFee .", ". $PaypalNet .", ". $this->UData['id'] .", '". $PaypalCustomerName ."', '". $PaypalCustomerEmail ."', ". time() .", '". $TransactionStatus ."')");

    }

    // Builds the message to return
    public function GetMessage($S, $B, $E = false){

        $C = ($E)? "danger" : "success";
        return '<div id="fb-message" class="alert alert-'. $C .'"><strong>'. $B .'</strong> '. $S .'</div>';

    }

    public function GivePLToUser($T){
        $this->DB->query("UPDATE users SET vip_points = vip_points + ". $T . " WHERE id = ". $this->UData['id'] . " LIMIT 1");
    }



}