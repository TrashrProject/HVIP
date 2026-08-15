<!-- admanager -->

<script type="text/javascript" src="https://kubbo.city/assets/scripts/plugins/toastr.min.js"></script>
<script type="text/javascript" src="https://kubbo.city/assets/scripts/plugins/toastr-settings.js"></script>
<script type="text/javascript" src="https://kubbo.city/assets/scripts/plugins/jquery.min.js"></script>
<script type="text/javascript" src="https://kubbo.city/assets/scripts/plugins/jquery-ui.js"></script>
<script async src="https://securepubads.g.doubleclick.net/tag/js/gpt.js"></script>
<script type="text/javascript">
	function divs() {
		var texto = "";
		var posible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

		for (var i = 0; i < 5; i++) texto += posible.charAt(Math.floor(Math.random() * posible.length));

		return texto;
	}

	var div2 = divs();
	window.googletag = window.googletag || { cmd: [] };
	googletag.cmd.push(function () {
		googletag.pubads().enableAsyncRendering();
		slot1 = googletag.defineSlot("/21636753996/client", [300, 250], div2).addService(googletag.pubads());
		googletag.enableServices();
	});

	$("#div-2").attr("id", div2);
</script>

<div id='div-2' style='min-width: 300px; min-height: 250px;'><script>googletag.cmd.push(function() { googletag.display(div2); });</script></div>
