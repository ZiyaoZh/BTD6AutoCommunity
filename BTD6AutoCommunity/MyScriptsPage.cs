using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BTD6AutoCommunity.Constants;

namespace BTD6AutoCommunity
{
    // 我的脚本页面
    public partial class BTD6AutoCommunity
    {
        private string currentDirectory;
        private void InitializeMyScriptsPage()
        {
            LoadDirectoryTree();
        }
        private void LoadDirectoryTree()
        {
            OperationsTV.Nodes.Clear();
            DirectoryInfo rootDir1 = new DirectoryInfo(@"data\我的脚本");
            DirectoryInfo rootDir2 = new DirectoryInfo(@"data\最近删除");
            TreeNode rootNode1 = new TreeNode(rootDir1.Name)
            {
                Tag = rootDir1
            };
            rootNode1.Nodes.Add("");
            OperationsTV.Nodes.Add(rootNode1);
            TreeNode rootNode2 = new TreeNode(rootDir2.Name)
            {
                Tag = rootDir2
            };
            rootNode2.Nodes.Add("");
            OperationsTV.Nodes.Add(rootNode2);
        }

        private void OperationsTV_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selectedNode = e.Node;
            if (selectedNode != null && selectedNode.Tag is DirectoryInfo info)
            {
                DirectoryInfo dirInfo = info;
                DisplayFiles(dirInfo.FullName);
                currentDirectory = dirInfo.FullName;
                //MessageBox.Show(dirInfo.FullName);
                LoadSubDirectories(selectedNode, dirInfo);
            }
        }

        private void LoadSubDirectories(TreeNode node, DirectoryInfo dirInfo)
        {
            //MessageBox.Show("正在加载目录：" + dirInfo.Name + node.Nodes.Count.ToString());
            if (node.Nodes.Count == 1 && node.Nodes[0].Text == "")
            {
                node.Nodes.Clear();
                try
                {
                    if (dirInfo.Name == "我的脚本")
                    {
                        foreach (Maps maps in Enum.GetValues(typeof(Maps)))
                        {
                            string subDirName = GetTypeName(maps);
                            DirectoryInfo subDir = new DirectoryInfo(Path.Combine(dirInfo.FullName, subDirName));
                            TreeNode subNode = new TreeNode(subDirName)
                            {
                                Tag = subDir
                            };
                            subNode.Nodes.Add(""); // Add dummy node to make it expandable
                            switch (GetMapType(maps))
                            {
                                case MapTypes.Expert:
                                    subNode.ForeColor = Color.Red;
                                    break;
                                case MapTypes.Advanced:
                                    subNode.ForeColor = Color.Purple;
                                    break;
                                case MapTypes.Intermediate:
                                    subNode.ForeColor = Color.Blue;
                                    break;
                                case MapTypes.Beginner:
                                    subNode.ForeColor = Color.Green;
                                    break;
                                default:
                                    subNode.ForeColor = Color.Black;
                                    break;
                            }
                            node.Nodes.Add(subNode);
                        }
                        return;
                    }
                    DirectoryInfo[] subDirs = dirInfo.GetDirectories();
                    foreach (DirectoryInfo subDir in subDirs)
                    {
                        TreeNode subNode = new TreeNode(subDir.Name)
                        {
                            Tag = subDir
                        };
                        subNode.Nodes.Add(""); // Add dummy node to make it expandable
                        node.Nodes.Add(subNode);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    // 创建目录
                    Directory.CreateDirectory(dirInfo.FullName);
                    MessageBox.Show("已自动创建新目录: " + dirInfo.FullName);
                    string[] subDirectories = { "简单", "中级", "困难" };
                    foreach (string subDirName in subDirectories)
                    {
                        Directory.CreateDirectory(Path.Combine(dirInfo.FullName, subDirName));
                    }
                    DirectoryInfo[] subDirs = dirInfo.GetDirectories();
                    foreach (DirectoryInfo subDir in subDirs)
                    {
                        TreeNode subNode = new TreeNode(subDir.Name)
                        {
                            Tag = subDir
                        };
                        subNode.Nodes.Add(""); // Add dummy node to make it expandable
                        node.Nodes.Add(subNode);
                    }
                }
            }
        }

        private void DisplayFiles(string path)
        {
            //MessageBox.Show("displayfile");
            OperationsLV.Items.Clear();
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                FileInfo[] files = dirInfo.GetFiles();
                //MessageBox.Show(files.Length.ToString());
                foreach (FileInfo file in files)
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                    ListViewItem item = new ListViewItem(fileNameWithoutExtension);
                    item.SubItems.Add(file.Length.ToString());
                    item.SubItems.Add(file.Extension);
                    item.Tag = file.FullName;
                    OperationsLV.Items.Add(item);
                    //OperationsLV.Refresh();
                }
            }
            catch
            {
                MessageBox.Show("无法打开目录: " + path);
            }
        }

        private void OperationsLV_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.Label != null && OperationsLV.Items[e.Item].Tag is string oldFilePath)
            {
                string fileExtension = Path.GetExtension(oldFilePath);
                //string oldFileName = Path.GetFileNameWithoutExtension(oldFilePath);
                string newFileName = e.Label + fileExtension;
                string newFilePath = Path.Combine(currentDirectory, newFileName);

                try
                {
                    File.Move(oldFilePath, newFilePath);
                    OperationsLV.Items[e.Item].Tag = newFilePath; // Update the tag to the new file path
                }
                catch (IOException ex)
                {
                    MessageBox.Show("An error occurred while renaming the file: " + ex.Message);
                    e.CancelEdit = true; // Cancel the edit operation
                }
            }
        }

        private void OperationsLV_ItemActivate(object sender, EventArgs e)
        {
            if (OperationsLV.SelectedItems.Count > 0)
            {
                string filePath = OperationsLV.SelectedItems[0].Tag as string;
                if (filePath != null)
                {
                    // 双击编辑
                    try
                    {
                        MyInstructions.Clear();

                        string jsonString = File.ReadAllText(filePath);
                        MyInstructions = JsonConvert.DeserializeObject<ScriptEditorSuite>(jsonString);
                        MyInstructions.ScriptName = Path.GetFileNameWithoutExtension(filePath);
                        MyInstructions.RepairScript();
                        LoadScriptInfo();
                        BindInstructionsViewTL(MyInstructions.Displayinstructions);
                        StartPrgramTC.SelectedIndex = 1;
                    }
                    catch
                    {
                        MessageBox.Show("请选择正确的脚本文件！");
                    }
                }
            }
        }

        private void ImportBT_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> eachDir = currentDirectory.Split('\\').ToList();
                string lastDir = eachDir[eachDir.Count - 1];
                if (lastDir == "简单" || lastDir == "中级" || lastDir == "困难")
                {
                    if (!string.IsNullOrEmpty(currentDirectory))
                    {
                        using (OpenFileDialog openFileDialog = new OpenFileDialog())
                        {
                            if (openFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                string sourceFilePath = openFileDialog.FileName;
                                string fileName = Path.GetFileName(sourceFilePath);
                                string destinationFilePath = Path.Combine(currentDirectory, fileName);
                                if (Path.GetExtension(fileName) == ".btd6")
                                {
                                    try
                                    {
                                        File.Copy(sourceFilePath, destinationFilePath, true);
                                        DisplayFiles(currentDirectory);
                                    }
                                    catch (IOException ex)
                                    {
                                        MessageBox.Show("脚本格式错误！" + ex.Message);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("脚本格式错误！");
                                }
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("请在左侧选择地图\\难度");
                    }
                }
                else
                {
                    MessageBox.Show("请在左侧选择地图\\难度");
                }
            }
            catch
            {
                MessageBox.Show("请在左侧选择地图\\难度");
            }

        }

        private void DeleteOperationBT_Click(object sender, EventArgs e)
        {
            if (OperationsLV.SelectedItems.Count > 0)
            {
                string filePath = OperationsLV.SelectedItems[0].Tag as string;
                if (filePath != null)
                {
                    try
                    {
                        // 判断是否是“最近删除”文件夹中的文件
                        if (filePath.Contains(@"data\最近删除\"))
                        {
                            // 直接删除文件
                            File.Delete(filePath);
                        }
                        else
                        {
                            string targetFolder = @"data\最近删除\";
                            string targetFileName = Path.GetFileName(filePath);
                            string targetFilePath = Path.Combine(targetFolder, targetFileName);
                            string newFileName = Path.GetFileNameWithoutExtension(targetFileName) +
                                                    "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") +
                            Path.GetExtension(targetFileName);
                            targetFilePath = Path.Combine(targetFolder, newFileName);
                            // 移动文件到“最近删除”文件夹
                            File.Move(filePath, targetFilePath);
                        }
                        DisplayFiles(currentDirectory);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show("An error occurred while deleting the file: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("请选择需要删除的脚本！");
            }
        }

        private void EditBT_Click(object sender, EventArgs e)
        {
            if (OperationsLV.SelectedItems.Count > 0)
            {
                string filePath = OperationsLV.SelectedItems[0].Tag as string;
                if (filePath != null)
                {
                    try
                    {
                        MyInstructions.Clear();
                        string jsonString = File.ReadAllText(filePath);

                        MyInstructions = JsonConvert.DeserializeObject<ScriptEditorSuite>(jsonString);
                        MyInstructions.ScriptName = Path.GetFileNameWithoutExtension(filePath);
                        MyInstructions.RepairScript();
                        LoadScriptInfo();
                        BindInstructionsViewTL(MyInstructions.Displayinstructions);
                        StartPrgramTC.SelectedIndex = 1;
                    }
                    catch
                    {
                        MessageBox.Show("文件打开失败！");
                    }
                }
            }
            else
            {
                MessageBox.Show("请选择需要编辑的脚本！");
            }
        }

        private void SelectPath(string path)
        {
            TreeNode node = FindNodeByPath(OperationsTV.Nodes, path);
            if (node != null)
            {
                OperationsTV.SelectedNode = node;
                node.EnsureVisible();
                OperationsTV.Focus();
                DisplayFiles(path);
            }
            else
            {
                MessageBox.Show("Treenode not found");
            }
            StartPrgramTC.SelectedIndex = 3;
        }

        private TreeNode FindNodeByPath(TreeNodeCollection nodes, string path)
        {
            // 一次性加载完整目录
            foreach (TreeNode node in nodes)
            {
                // 确保仅在需要时或适当时加载
                if (node.Tag is DirectoryInfo dirInfo)
                {

                    // 显示当前节点路径和目标路径
                    //if (dirInfo != null)
                    //{
                    //    MessageBox.Show("当前节点路径: " + dirInfo.FullName + "\n目标路径: " + path);
                    //}
                    // 这样只在第一次调用时加载子目录，避免重复加载
                    LoadSubDirectories(node, dirInfo);

                    // 检查路径
                    if (dirInfo.FullName.Equals(path, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return node;
                    }
                }

                // 递归查找
                TreeNode foundNode = FindNodeByPath(node.Nodes, path);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            return null;
        }

        private void RunBT_Click(object sender, EventArgs e)
        {
            if (OperationsLV.SelectedItems.Count > 0)
            {
                if (OperationsLV.SelectedItems[0].Tag is string filePath)
                {
                    try
                    {
                        string[] directoryName = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        int lastIndex = directoryName.Length - 1;
                        ExecuteMapCB.SelectedIndex = ExecuteMapCB.FindString(directoryName[lastIndex - 2]);
                        ExecuteDifficultyCB.SelectedIndex = ExecuteDifficultyCB.FindString(directoryName[lastIndex - 1]);
                        ExecuteDifficultyCB_SelectedIndexChanged(ExecuteDifficultyCB, EventArgs.Empty);
                        ExecuteScriptCB.SelectedIndex = ExecuteScriptCB.FindString(Path.GetFileNameWithoutExtension(directoryName[lastIndex]));

                        StartPrgramTC.SelectedIndex = 0;
                        ExecuteModeCB.SelectedIndex = 0;
                        StartProgramBT.PerformClick();

                    }
                    catch
                    {
                        MessageBox.Show("文件打开失败！");
                    }
                }
            }
            else
            {
                MessageBox.Show("请选择需要运行的脚本！");
            }
        }
    }
}
