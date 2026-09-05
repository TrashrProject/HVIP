using System.Diagnostics;
using System.Drawing;

namespace ParadiseRP.EmulatorManager;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new ManagerForm());
    }
}

internal sealed class ManagerForm : Form
{
    private const int GamePort = 30000;
    private const int WebSocketPort = 2096;
    private readonly string _repositoryRoot;
    private readonly string _runtimeDirectory;
    private readonly string _emulatorJar;
    private readonly string _javaExecutable;
    private readonly Label _statusLabel;
    private readonly Label _detailLabel;
    private readonly Button _startButton;
    private readonly Button _restartButton;
    private readonly System.Windows.Forms.Timer _statusTimer;
    private bool _operationRunning;

    public ManagerForm()
    {
        _repositoryRoot = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        _runtimeDirectory = Path.Combine(_repositoryRoot, "runtime", "WavePlus");
        _emulatorJar = Path.Combine(_runtimeDirectory, "WaveRP-Arcturus.jar");
        _javaExecutable = @"C:\Program Files\Android\openjdk\jdk-21.0.8\bin\java.exe";

        Text = "ParadiseRP - Gestion de l'émulateur";
        ClientSize = new Size(560, 330);
        MinimumSize = new Size(576, 369);
        MaximumSize = new Size(576, 369);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(7, 28, 54);
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F);

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 92,
            BackColor = Color.FromArgb(10, 66, 111)
        };
        Controls.Add(header);

        var title = new Label
        {
            AutoSize = true,
            Text = "PARADISERP",
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(24, 15)
        };
        header.Controls.Add(title);

        var subtitle = new Label
        {
            AutoSize = true,
            Text = "Gestion de l'émulateur WaveRP",
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(165, 220, 255),
            Location = new Point(27, 59)
        };
        header.Controls.Add(subtitle);

        _statusLabel = new Label
        {
            AutoSize = false,
            Size = new Size(510, 32),
            Location = new Point(25, 112),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            BackColor = Color.FromArgb(14, 44, 77)
        };
        Controls.Add(_statusLabel);

        _detailLabel = new Label
        {
            AutoSize = false,
            Size = new Size(510, 38),
            Location = new Point(25, 148),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(190, 211, 230),
            Text = "Vérification du serveur..."
        };
        Controls.Add(_detailLabel);

        _startButton = CreateButton("ALLUMER L'ÉMULATEUR", new Point(25, 205), Color.FromArgb(30, 165, 92));
        _startButton.Click += async (_, _) => await StartEmulatorAsync();
        Controls.Add(_startButton);

        _restartButton = CreateButton("REDÉMARRER L'ÉMULATEUR", new Point(285, 205), Color.FromArgb(18, 128, 200));
        _restartButton.Click += async (_, _) => await RestartEmulatorAsync();
        Controls.Add(_restartButton);

        var note = new Label
        {
            AutoSize = false,
            Size = new Size(510, 42),
            Location = new Point(25, 270),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(125, 160, 190),
            Font = new Font("Segoe UI", 8.5F),
            Text = "Le redémarrage déconnecte brièvement les joueurs connectés."
        };
        Controls.Add(note);

        _statusTimer = new System.Windows.Forms.Timer { Interval = 2500 };
        _statusTimer.Tick += (_, _) => RefreshStatus();
        _statusTimer.Start();
        Shown += (_, _) => RefreshStatus();
    }

    private static Button CreateButton(string text, Point location, Color color)
    {
        var button = new Button
        {
            Text = text,
            Location = location,
            Size = new Size(250, 52),
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = ControlPaint.Light(color);
        button.FlatAppearance.BorderSize = 1;
        return button;
    }

    private void RefreshStatus()
    {
        if (_operationRunning)
            return;

        int? pid = FindEmulatorProcessId();
        if (pid.HasValue)
        {
            _statusLabel.Text = "● ÉMULATEUR ALLUMÉ";
            _statusLabel.ForeColor = Color.FromArgb(75, 224, 140);
            _detailLabel.Text = $"Serveur opérationnel — PID {pid.Value}";
            _startButton.Enabled = false;
            _restartButton.Enabled = true;
        }
        else
        {
            _statusLabel.Text = "● ÉMULATEUR ÉTEINT";
            _statusLabel.ForeColor = Color.FromArgb(245, 105, 105);
            _detailLabel.Text = "Aucun serveur WaveRP détecté";
            _startButton.Enabled = true;
            _restartButton.Enabled = true;
        }
    }

    private async Task StartEmulatorAsync()
    {
        if (FindEmulatorProcessId().HasValue)
        {
            ShowInformation("L'émulateur est déjà allumé.");
            RefreshStatus();
            return;
        }

        await RunOperationAsync("Démarrage de WaveRP...", StartAndWaitAsync);
    }

    private async Task RestartEmulatorAsync()
    {
        DialogResult answer = MessageBox.Show(
            "Redémarrer WaveRP maintenant ? Les joueurs connectés seront brièvement déconnectés.",
            "Confirmation du redémarrage",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes)
            return;

        await RunOperationAsync("Redémarrage de WaveRP...", async () =>
        {
            int? pid = FindEmulatorProcessId();
            if (pid.HasValue)
            {
                Process process = Process.GetProcessById(pid.Value);
                if (!process.ProcessName.Equals("java", StringComparison.OrdinalIgnoreCase) &&
                    !process.ProcessName.Equals("javaw", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Le port du jeu appartient à un processus inattendu : {process.ProcessName} ({pid.Value}).");

                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
            }

            await StartAndWaitAsync();
        });
    }

    private async Task RunOperationAsync(string message, Func<Task> operation)
    {
        _operationRunning = true;
        _startButton.Enabled = false;
        _restartButton.Enabled = false;
        _statusLabel.Text = message;
        _statusLabel.ForeColor = Color.FromArgb(255, 205, 75);
        _detailLabel.Text = "Merci de patienter...";

        try
        {
            await operation();
            RefreshStatusAfterOperation();
            ShowInformation("L'émulateur ParadiseRP est opérationnel.");
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Erreur WaveRP", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _operationRunning = false;
            RefreshStatus();
        }
    }

    private async Task StartAndWaitAsync()
    {
        if (!File.Exists(_javaExecutable))
            throw new FileNotFoundException("Java 21 est introuvable.", _javaExecutable);
        if (!File.Exists(_emulatorJar))
            throw new FileNotFoundException("Le JAR WaveRP est introuvable.", _emulatorJar);

        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        string stdout = Path.Combine(_runtimeDirectory, $"manager-start-{stamp}.stdout.log");
        string stderr = Path.Combine(_runtimeDirectory, $"manager-start-{stamp}.stderr.log");
        string command =
            $"Start-Process -FilePath '{EscapePowerShell(_javaExecutable)}' " +
            "-ArgumentList '-jar','WaveRP-Arcturus.jar' " +
            $"-WorkingDirectory '{EscapePowerShell(_runtimeDirectory)}' " +
            $"-RedirectStandardOutput '{EscapePowerShell(stdout)}' " +
            $"-RedirectStandardError '{EscapePowerShell(stderr)}' -WindowStyle Hidden";

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-Command");
        startInfo.ArgumentList.Add(command);

        using Process launcher = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Impossible de lancer PowerShell.");
        await launcher.WaitForExitAsync();
        if (launcher.ExitCode != 0)
            throw new InvalidOperationException($"Le lancement de WaveRP a échoué (code {launcher.ExitCode}). Consulte {stderr}.");

        DateTime deadline = DateTime.UtcNow.AddSeconds(60);
        while (DateTime.UtcNow < deadline)
        {
            if (FindEmulatorProcessId().HasValue)
                return;
            await Task.Delay(1000);
        }

        throw new TimeoutException($"WaveRP n'a pas ouvert le port {GamePort} dans le délai prévu. Consulte {stderr}.");
    }

    private int? FindEmulatorProcessId()
    {
        int? pid = FindListeningProcessId(GamePort);
        return pid ?? FindListeningProcessId(WebSocketPort);
    }

    private static int? FindListeningProcessId(int port)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-Command");
        startInfo.ArgumentList.Add($"Get-NetTCPConnection -State Listen -LocalPort {port} -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty OwningProcess");

        using Process? process = Process.Start(startInfo);
        if (process is null)
            return null;
        string output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit(4000);
        return int.TryParse(output, out int pid) ? pid : null;
    }

    private void RefreshStatusAfterOperation()
    {
        _operationRunning = false;
        RefreshStatus();
        _operationRunning = true;
    }

    private static string EscapePowerShell(string value) => value.Replace("'", "''");

    private void ShowInformation(string message) =>
        MessageBox.Show(message, "ParadiseRP", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
