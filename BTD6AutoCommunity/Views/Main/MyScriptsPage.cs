using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine;
using BTD6AutoCommunity.GameObjects;
using BTD6AutoCommunity.ScriptEngine.ScriptSystem;

namespace BTD6AutoCommunity.Views.Main
{
    // 我的脚本页面
    public partial class BTD6AutoUI
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
                        foreach (Maps maps in Constants.MapsList)
                        {
                            string subDirName = Constants.GetTypeName(maps);
                            DirectoryInfo subDir = new DirectoryInfo(Path.Combine(dirInfo.FullName, subDirName));
                            TreeNode subNode = new TreeNode(subDirName)
                            {
                                Tag = subDir
                            };
                            subNode.Nodes.Add(""); // Add dummy node to make it expandable
                            switch (Constants.GetMapType(maps))
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
                if (OperationsLV.SelectedItems[0].Tag is string filePath)
                {
                    // 双击编辑
                    try
                    {
                        scriptEditorViewModel.EditScriptCommand.Execute(filePath);
                        StartPrgramTC.SelectedIndex = 1;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("请选择正确的脚本文件！" + ex.Message);
                    }
                }
            }
        }

        private void ImportBT_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string sourceFilePath = openFileDialog.FileName;
                    string fileName = Path.GetFileName(sourceFilePath);
                    string destinationFilePath = ScriptFileManager.GetScriptFullPath(ScriptFileManager.LoadScript(sourceFilePath));
                    if (Path.GetExtension(fileName) == ".btd6")
                    {
                        try
                        {
                            File.Copy(sourceFilePath, destinationFilePath, true);
                            SelectPath(Path.GetDirectoryName(destinationFilePath));
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

        private void OutputBT_Click(object sender, EventArgs e)
        {
            // 导出脚本
            if (OperationsLV.SelectedItems.Count > 0)
            {
                if (OperationsLV.SelectedItems[0].Tag is string filePath)
                {
                    try
                    {
                        // 把选择的脚本文件复制导出到本地
                        using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                        {
                            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(filePath) + ".btd6";
                            saveFileDialog.Filter = "脚本文件(*.btd6)|*.btd6";
                            saveFileDialog.InitialDirectory = currentDirectory;
                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                string destinationFilePath = saveFileDialog.FileName;
                                File.Copy(filePath, destinationFilePath, true);
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show("文件打开失败！");
                    }
                }
            }
            else
            {
                MessageBox.Show("请选择需要导出脚本的脚本！");
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
                if (OperationsLV.SelectedItems[0].Tag is string filePath)
                {
                    try
                    {
                        scriptEditorViewModel.EditScriptCommand.Execute(filePath);
                        StartPrgramTC.SelectedIndex = 1;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("请选择正确的脚本文件！" + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("请选择需要编辑的脚本！");
            }
        }

        /// <summary>
        /// 根据path选择TreeView节点，并显示其下的文件列表
        /// </summary>
        /// <param name="path"></param>
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
        }

        private TreeNode FindNodeByPath(TreeNodeCollection nodes, string path)
        {
            // 一次性加载完整目录
            foreach (TreeNode node in nodes)
            {
                // 确保仅在需要时或适当时加载
                if (node.Tag is DirectoryInfo dirInfo)
                {
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
                        startViewModel.SelectScriptCommand.Execute(filePath);

                        ExecuteModeCB.SelectedIndex = 0;
                        StartPrgramTC.SelectedIndex = 0;
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
