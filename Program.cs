using System.Windows.Forms;

AssemblyResolver.Register();

if (args.Length > 0)
{
    return CommandLineRunner.Run(args);
}

Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
Application.ThreadException += (_, eventArgs) =>
{
    MessageBox.Show(eventArgs.Exception.ToString(), "Error inesperado", MessageBoxButtons.OK, MessageBoxIcon.Error);
};

Application.Run(new MainForm());
return 0;
