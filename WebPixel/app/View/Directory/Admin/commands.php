<?php
$repoRoot = dirname(__DIR__, 5);
$commandFile = $repoRoot . DS . 'RDP EMU header change' . DS . 'HabboHotel' . DS . 'Rooms' . DS . 'Chat' . DS . 'Commands' . DS . 'CommandManager.cs';
$commands = array();
$sourceAvailable = is_file($commandFile);
$q = strtolower(trim((string)($_GET['q'] ?? '')));

if ($sourceAvailable) {
    $source = (string)file_get_contents($commandFile);

    if (preg_match_all('/this\.Register\("([^"]+)"\s*,\s*new\s+([A-Za-z0-9_]+)\(\)(?:\s*,\s*"([^"]+)")?\s*\);/', $source, $matches, PREG_SET_ORDER)) {
        foreach ($matches as $match) {
            $commands[] = array(
                'command' => $match[1],
                'handler' => $match[2],
                'log' => isset($match[3]) ? $match[3] : ''
            );
        }
    }

    if ($q !== '') {
        $commands = array_values(array_filter($commands, function ($command) use ($q) {
            return strpos(strtolower($command['command']), $q) !== false
                || strpos(strtolower($command['handler']), $q) !== false;
        }));
    }

    usort($commands, function ($a, $b) {
        return strcmp($a['command'], $b['command']);
    });
}
?>
<section class="pcc-alert info">
    <i class="fas fa-code"></i>
    <div>
        <strong>Registry réelle, pas une liste hardcodée</strong>
        <p>Le module lit directement les appels <code>Register()</code> de <code>CommandManager.cs</code> présents sur le serveur. Les commandes apparaissent donc seulement si elles sont réellement enregistrées dans le core déployé.</p>
    </div>
</section>

<?php if (!$sourceAvailable): ?>
    <section class="pcc-panel">
        <div class="pcc-empty">
            <i class="fas fa-terminal"></i>
            <strong>Source CommandManager non disponible sur ce déploiement</strong>
            <span>Le CMS ne fabrique pas une registry de substitution.</span>
        </div>
    </section>
    <?php return; ?>
<?php endif; ?>

<section class="pcc-panel">
    <form class="pcc-filterbar">
        <input type="hidden" name="page" value="commands">
        <div class="pcc-search-field">
            <i class="fas fa-search"></i>
            <input name="q" value="<?php echo $h($q); ?>" placeholder="Commande ou handler C#…">
        </div>
        <button class="pcc-button secondary">Rechercher</button>
        <span class="pcc-filter-count"><?php echo $number(count($commands)); ?> alias enregistré(s)</span>
    </form>

    <div class="pcc-table-wrap">
        <table class="pcc-table pcc-table-dense">
            <thead>
                <tr>
                    <th>Commande</th>
                    <th>Handler réel</th>
                    <th>Journal spécial</th>
                    <th>Activation</th>
                    <th>Source</th>
                </tr>
            </thead>
            <tbody>
                <?php foreach ($commands as $command): ?>
                    <tr>
                        <td><code>:<?php echo $h($command['command']); ?></code></td>
                        <td><?php echo $h($command['handler']); ?></td>
                        <td><?php echo $command['log'] !== '' ? '<span class="pcc-badge warning">' . $h($command['log']) . '</span>' : '—'; ?></td>
                        <td><span class="pcc-badge success">ENREGISTRÉE</span></td>
                        <td><code>CommandManager.cs</code></td>
                    </tr>
                <?php endforeach; ?>
            </tbody>
        </table>
    </div>
</section>

<section class="pcc-alert warning">
    <i class="fas fa-lock"></i>
    <div>
        <strong>Code des commandes non éditable depuis le CMS</strong>
        <p>La permission exacte est portée par les classes <code>IChatCommand</code> et le système Habbo permissions. V3 ne permet pas d’éditer le C# depuis l’admin. Un futur manifest partagé pourra exposer description/catégorie/cooldown sans dupliquer l’implémentation.</p>
    </div>
</section>
