<?php
	include('PdfToText.phpclass');

	$Min = 270000;
	$Max = 300000;

	$Count = 0;
	
	set_time_limit(86400);
	for($i = $Min; $i <= $Max; $i++)
	{
		$PDFName = "LSM" . $i;

		$url  = 'http://189.212.132.188/WSDLGenericNew/folios/' . $PDFName . '.pdf';

		if(url_exists($url))
		{
			$pdf  =  new PdfToText ($url) ;
			$Text = $pdf->Text ;

			if(strpos($Text, '25 de Noviembre del 2020') !== FALSE || strpos($Text, '26 de Noviembre del 2020') !== FALSE || strpos($Text, '27 de Noviembre del 2020') !== FALSE || strpos($Text, '28 de Noviembre del 2020') !== FALSE)
			{
				$path = "C:\\inetpub\\WebPixel\\pdfs\\" . $PDFName . ".pdf";

				$ch = curl_init($url);
				curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
				curl_setopt($ch, CURLOPT_REFERER, $url);

				$data = curl_exec($ch);

				curl_close($ch);

				$result = file_put_contents($path, $data);

				$Count++;
			}
		}
	}

	echo "Se descargaron <b>" . $Count . " de " . (($Max - $Min) + 1) . "</b> PDFS consultados.";

	function url_exists($url) {
	    $headers = @get_headers($url);
	    return (strpos($headers[0],'200')===false)? false:true;
	}
?>