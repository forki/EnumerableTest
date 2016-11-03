﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace EnumerableTest.Runner.Wpf
{
    /// <summary>
    /// TestTreeView.xaml の相互作用ロジック
    /// </summary>
    public partial class TestTreeView : UserControl
    {
        readonly TestTree testTree = new TestTree();

        public TestTreeView()
        {
            InitializeComponent();

            var args = Environment.GetCommandLineArgs();
#if DEBUG
            args = new[] { @"..\..\..\EnumerableTest.Sandbox\bin\Debug\EnumerableTest.Sandbox.dll" };
#endif
            foreach (var arg in args)
            {
                testTree.LoadFile(new FileInfo(arg));
            }

            DataContext = testTree;
            Unloaded += (sender, e) => testTree.Dispose();
        }
    }
}