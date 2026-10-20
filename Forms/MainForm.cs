using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Ookii.Dialogs.WinForms;

internal sealed class MainForm : Form
{
    private readonly ListBox inputListBox = new()
    {
        Dock = DockStyle.Fill,
        SelectionMode = SelectionMode.One,
        IntegralHeight = false,
    };
    private readonly TextBox outputFolderTextBox = new() { Dock = DockStyle.Fill };
    private readonly ComboBox versionComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
    private readonly Label statusLabel = new() { AutoSize = true, Text = "Agrega uno o varios PDFs para comenzar." };

    public MainForm()
    {
        Text = "Convertidor PDF";
        Width = 780;
        Height = 440;
        MinimumSize = new Size(680, 360);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 9F);

        versionComboBox.Items.AddRange(new object[] { "1.2", "1.3", "1.4", "1.5", "1.6", "1.7" });
        versionComboBox.SelectedItem = "1.4";

        var inputButton = new Button { Text = "Agregar PDF", AutoSize = true };
        inputButton.Click += (_, _) => SelectInput();
        var removeButton = new Button { Text = "Quitar seleccionado", AutoSize = true };
        removeButton.Click += (_, _) => RemoveSelectedInput();
        var outputButton = new Button { Text = "Carpeta destino", AutoSize = true };
        outputButton.Click += (_, _) => SelectOutputFolder();
        var convertButton = new Button
        {
            Text = "Convertir PDF",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
        };
        convertButton.Click += (_, _) => Convert();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 3,
            RowCount = 6,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        for (var row = 1; row < layout.RowCount; row++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        inputListBox.MinimumSize = new Size(300, 190);
        layout.Controls.Add(
            new Label
            {
                Text = "PDFs de entrada:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
            },
            0,
            0
        );
        layout.Controls.Add(inputListBox, 1, 0);
        layout.Controls.Add(inputButton, 2, 0);
        layout.Controls.Add(removeButton, 2, 1);
        layout.Controls.Add(
            new Label
            {
                Text = "Carpeta salida:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
            },
            0,
            2
        );
        layout.Controls.Add(outputFolderTextBox, 1, 2);
        layout.Controls.Add(outputButton, 2, 2);
        layout.Controls.Add(
            new Label
            {
                Text = "Versión PDF:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
            },
            0,
            3
        );
        layout.Controls.Add(versionComboBox, 1, 3);
        layout.Controls.Add(convertButton, 1, 4);
        layout.Controls.Add(statusLabel, 1, 5);
        Controls.Add(layout);
    }

    private void SelectInput()
    {
        var initialDirectory = Directory.Exists(outputFolderTextBox.Text.Trim())
            ? outputFolderTextBox.Text.Trim()
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var pickerThread = new Thread(() =>
        {
            try
            {
                string[] selectedFiles = Array.Empty<string>();
                using (
                    var dialog = new OpenFileDialog
                    {
                        Filter = "Archivos PDF (*.pdf)|*.pdf|Todos los archivos (*.*)|*.*",
                        Multiselect = true,
                        CheckFileExists = true,
                        CheckPathExists = true,
                        RestoreDirectory = true,
                        InitialDirectory = initialDirectory,
                        Title = "Seleccionar uno o varios archivos PDF",
                    }
                )
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        selectedFiles = dialog.FileNames;
                    }
                }
                BeginInvoke(new Action(() => AddInputFiles(selectedFiles)));
            }
            catch (Exception exception)
            {
                BeginInvoke(new Action(() => ShowError($"No se pudo abrir el selector de archivos:\n{exception.Message}")));
            }
        });
        pickerThread.SetApartmentState(ApartmentState.STA);
        pickerThread.IsBackground = true;
        pickerThread.Start();
    }

    private void AddInputFiles(string[] fileNames)
    {
        foreach (var fileName in fileNames)
        {
            if (!inputListBox.Items.Contains(fileName))
            {
                inputListBox.Items.Add(fileName);
            }
        }
        if (fileNames.Length > 0 && string.IsNullOrWhiteSpace(outputFolderTextBox.Text))
        {
            outputFolderTextBox.Text = Path.GetDirectoryName(fileNames[0]) ?? Environment.CurrentDirectory;
        }
    }

    private void RemoveSelectedInput()
    {
        if (inputListBox.SelectedIndex >= 0)
        {
            inputListBox.Items.RemoveAt(inputListBox.SelectedIndex);
        }
    }

    private void SelectOutputFolder()
    {
        var initialDirectory = Directory.Exists(outputFolderTextBox.Text.Trim())
            ? outputFolderTextBox.Text.Trim()
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var pickerThread = new Thread(() =>
        {
            try
            {
                string? selectedPath = null;
                using (
                    var dialog = new VistaFolderBrowserDialog
                    {
                        SelectedPath = initialDirectory,
                        Description = "Seleccionar carpeta donde guardar los PDFs convertidos",
                        UseDescriptionForTitle = true,
                    }
                )
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        selectedPath = dialog.SelectedPath;
                    }
                }

                BeginInvoke(
                    new Action(() =>
                    {
                        if (!string.IsNullOrWhiteSpace(selectedPath))
                        {
                            outputFolderTextBox.Text = selectedPath;
                        }
                    })
                );
            }
            catch (Exception exception)
            {
                BeginInvoke(new Action(() => ShowError($"No se pudo abrir el selector de carpetas:\n{exception.Message}")));
            }
        });
        pickerThread.SetApartmentState(ApartmentState.STA);
        pickerThread.IsBackground = true;
        pickerThread.Start();
    }

    private void Convert()
    {
        if (inputListBox.Items.Count == 0)
        {
            ShowError("Agrega al menos un PDF de entrada.");
            return;
        }
        var outputFolder = outputFolderTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(outputFolder))
        {
            ShowError("Indica una carpeta de salida.");
            return;
        }
        try
        {
            Directory.CreateDirectory(outputFolder);
            var version = PdfConverter.ParseVersion(versionComboBox.Text);
            var converted = 0;
            foreach (string inputPath in inputListBox.Items)
            {
                var outputPath = Path.Combine(outputFolder, $"{Path.GetFileNameWithoutExtension(inputPath)}_compatible.pdf");
                if (string.Equals(Path.GetFullPath(inputPath), Path.GetFullPath(outputPath), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"La salida coincide con la entrada: {inputPath}");
                }
                PdfConverter.Convert(Path.GetFullPath(inputPath), outputPath, version);
                converted++;
            }
            statusLabel.Text = $"Conversión terminada: {converted} PDF(s).";
            MessageBox.Show(
                this,
                $"Se convirtieron {converted} PDF(s) correctamente.",
                "Listo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (Exception exception)
        {
            ShowError($"No se pudo convertir el PDF: {exception.Message}");
        }
    }

    private void ShowError(string message)
    {
        statusLabel.Text = message;
        MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
