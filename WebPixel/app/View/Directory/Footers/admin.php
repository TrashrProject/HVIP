<script>
window.PCC = <?php echo json_encode(array(
    'searchUrl' => Config::$URL . '/admin.php?ajax=global-search&q=',
    'baseUrl' => Config::$URL,
    'csrf' => $PCC->csrfToken()
), JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE); ?>;
</script>
<script src="<?php echo JS; ?>/admin-control-center.js?v=<?php echo rawurlencode(Config::$V); ?>"></script>
</body>
</html>
