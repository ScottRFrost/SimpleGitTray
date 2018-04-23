using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace SimpleGitTray
{
    public partial class form : Form
    {
        public Config config;

        [STAThread]
        public static void Main()
        {
            Application.Run(new form());
        }

        public form()
        {
            InitializeComponent();
            config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText("config.json"));
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);

            trayIcon.BalloonTipTitle = "SimpleGitTray Loaded";
            trayIcon.BalloonTipText = "Fetching from " + config.Repos.Length + " repos every " + config.FetchMinutes + " minutes";
            trayIcon.ShowBalloonTip(1000);

            Fetch();
        }

        public void Fetch()
        {
            // Clear Menu
            while (menu.Items.Count > 3)
            {
                menu.Items.RemoveAt(3);
            }

            foreach (Repos r in config.Repos)
            {
                menu.Items.Add(new ToolStripLabel(r.Name));


                using (var repo = new Repository(r.LocalPath))
                {
                    // Fetch
                    //foreach (Remote remote in repo.Network.Remotes)
                    //{
                    //    IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                    //    Commands.Fetch(repo, remote.Name, refSpecs, null, null);
                    //}

                    // Check for local changes
                    var fileStatuses = new Dictionary<FileStatus, int>();
                    foreach (StatusEntry statusEntry in repo.RetrieveStatus())
                    {
                        if (!fileStatuses.ContainsKey(statusEntry.State))
                        {
                            fileStatuses.Add(statusEntry.State, 1);
                        }
                        else
                        {
                            fileStatuses[statusEntry.State]++;
                        }
                    }
                    if (fileStatuses.ContainsKey(FileStatus.Nonexistent))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.Nonexistent] + " Non-Existent"));
                    if (fileStatuses.ContainsKey(FileStatus.NewInIndex))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.NewInIndex] + " New"));
                    if (fileStatuses.ContainsKey(FileStatus.ModifiedInIndex))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.ModifiedInIndex] + " Modified"));
                    if (fileStatuses.ContainsKey(FileStatus.DeletedFromIndex))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.DeletedFromIndex] + " Deleted"));
                    if (fileStatuses.ContainsKey(FileStatus.RenamedInIndex))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.RenamedInIndex] + " Renamed"));
                    if (fileStatuses.ContainsKey(FileStatus.TypeChangeInIndex))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.TypeChangeInIndex] + " Type Changed"));
                    if (fileStatuses.ContainsKey(FileStatus.NewInWorkdir))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.NewInWorkdir] + " New (Uncommitted)"));
                    if (fileStatuses.ContainsKey(FileStatus.ModifiedInWorkdir))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.ModifiedInWorkdir] + " Modified (Uncommitted)"));
                    if (fileStatuses.ContainsKey(FileStatus.DeletedFromWorkdir))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.DeletedFromWorkdir] + " Deleted (Uncommitted)"));
                    if (fileStatuses.ContainsKey(FileStatus.TypeChangeInWorkdir))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.TypeChangeInWorkdir] + " Type Changed (Uncommitted)"));
                    if (fileStatuses.ContainsKey(FileStatus.RenamedInWorkdir))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.RenamedInWorkdir] + " Renamed (Uncommitted)"));
                    if (fileStatuses.ContainsKey(FileStatus.Unreadable))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.Unreadable] + " Unreadable (In Working Dir)"));
                    if (fileStatuses.ContainsKey(FileStatus.Conflicted))
                        menu.Items.Add(new ToolStripLabel(fileStatuses[FileStatus.Conflicted] + " Conflicted"));

                    // Check for diffs vs remote (might need to do them in reverse order too)
                    var headTree = repo.Head.Tip.Tree;
                    var remoteMasterTree = repo.Branches["origin/master"].Tip.Tree;
                    var diffs = new Dictionary<ChangeKind, int>();
                    foreach (TreeEntryChanges treeEntryChange in repo.Diff.Compare<TreeChanges>(remoteMasterTree, headTree))
                    {
                        if (!diffs.ContainsKey(treeEntryChange.Status))
                        {
                            diffs.Add(treeEntryChange.Status, 1);
                        }
                        else
                        {
                            diffs[treeEntryChange.Status]++;
                        }
                    }

                    if (diffs.ContainsKey(ChangeKind.Added))
                        menu.Items.Add(new ToolStripLabel(diffs[ChangeKind.Added] + " Added (Diff)"));
                    if (diffs.ContainsKey(ChangeKind.Deleted))
                        menu.Items.Add(new ToolStripLabel(diffs[ChangeKind.Deleted] + " Deleted (Diff)"));
                    if (diffs.ContainsKey(ChangeKind.Modified))
                        menu.Items.Add(new ToolStripLabel(diffs[ChangeKind.Modified] + " Modified (Diff)"));
                    if (diffs.ContainsKey(ChangeKind.Renamed))
                        menu.Items.Add(new ToolStripLabel(diffs[ChangeKind.Renamed] + " Renamed (Diff)"));
                    if (diffs.ContainsKey(ChangeKind.Copied))
                        menu.Items.Add(new ToolStripLabel(diffs[ChangeKind.Copied] + " Copied (Diff)"));
                    if (diffs.ContainsKey(ChangeKind.Ignored))
                        menu.Items.Add(new ToolStripLabel(diffs[ChangeKind.Ignored] + " Ignored (Diff)"));
                    if (diffs.ContainsKey(ChangeKind.Untracked))
                        menu.Items.Add(new ToolStripLabel(diffs[ChangeKind.Untracked] + " Untracked (Diff)"));
                    if (diffs.ContainsKey(ChangeKind.TypeChanged))
                        menu.Items.Add(new ToolStripLabel(diffs[ChangeKind.TypeChanged] + " TypeChanged (Diff)"));
                    if (diffs.ContainsKey(ChangeKind.Unreadable))
                        menu.Items.Add(new ToolStripLabel(diffs[ChangeKind.Unreadable] + " Unreadable (Diff)"));
                    if (diffs.ContainsKey(ChangeKind.Conflicted))
                        menu.Items.Add(new ToolStripLabel(diffs[ChangeKind.Conflicted] + " Conflicted (Diff)"));

                }
                menu.Items.Add(new ToolStripSeparator());
            }
            menu.Items.RemoveAt(menu.Items.Count - 1); // Remove trailing separator
        }

        #region "Menu"
        private void trayIcon_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    Visible = true;
                    break;
                case MouseButtons.Right:
                    menu.Show();
                    break;
            }
        }

        private void menuConfiguration_Click(object sender, EventArgs e)
        {
            Process.Start("config.json");
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region "Config Structs"
        public struct Repos
        {
            public string Name;
            public string URL;
            public string Branch;
            public string Username;
            public string Password;
            public string LocalPath;
        }

        public struct Config
        {
            public int FetchMinutes;
            public Repos[] Repos;
        }
        #endregion
    }
}
