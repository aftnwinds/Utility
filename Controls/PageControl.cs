﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Insight.Utils.Controls
{
    public partial class PageControl : XtraUserControl
    {
        private int _Handle;
        private int _PageSize;
        private int _Rows;
        private int _TotalPages = 1;
        private int _Current;
        private Collection<string> _PageSizes = new Collection<string> {"20", "40", "60", "80", "100"};

        /// <summary>  
        /// 每页显示行数发生改变，通知修改每页显示行数
        /// </summary>  
        public event PageSizeHandle PageSizeChanged;

        /// <summary>  
        /// 当前页发生改变，通知重新加载列表数据
        /// </summary>  
        public event PageReloadHandle CurrentPageChanged;

        /// <summary>  
        /// 列表总行数发生改变，通知修改FocusedRowHandle
        /// </summary>  
        public event TotalRowsHandle TotalRowsChanged;

        /// <summary>
        /// 表示将处理每页显示行数改变事件的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void PageSizeHandle(object sender, PageSizeEventArgs e);

        /// <summary>
        /// 表示将处理列表数据需重新加载事件的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void PageReloadHandle(object sender, PageControlEventArgs e);

        /// <summary>
        /// 表示将处理列表总行数改变事件的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void TotalRowsHandle(object sender, PageControlEventArgs e);

        /// <summary>
        /// 当前选中行Handle
        /// </summary>
        public int FocusedRowHandle
        {
            get => _Handle - _PageSize*_Current;
            set => _Handle = _PageSize*_Current + value;
        }

        /// <summary>
        /// 每页行数下拉列表选项
        /// </summary>
        public Collection<string> PageSizeItems
        {
            get => _PageSizes;
            set
            {
                _PageSizes = value;
                cbeRows.Properties.Items.AddRange(value);
                cbeRows.SelectedIndex = 0;
                _PageSize = int.Parse(_PageSizes[0]);
            }
        }

        /// <summary>
        /// 总行数
        /// </summary>
        public int TotalRows
        {
            get => _Rows;
            set
            {
                _Rows = value;
                _TotalPages = (int) Math.Ceiling((decimal) _Rows/_PageSize);
                Refresh();
            }
        }

        /// <summary>
        /// 当前页
        /// </summary>
        public int CurrentPage => _Current + 1;

        /// <summary>
        /// 构造方法
        /// </summary>
        public PageControl()
        {
            InitializeComponent();

            // 订阅控件按钮事件
            cbeRows.EditValueChanged += (sender, args) => PageRowsChanged();
            btnFirst.Click += (sender, args) => ChangePage(0);
            btnPrev.Click += (sender, args) => ChangePage(_Current - 1);
            btnNext.Click += (sender, args) => ChangePage(_Current + 1);
            btnLast.Click += (sender, args) => ChangePage(_TotalPages - 1);
        }

        /// <summary>
        /// 增加列表成员
        /// </summary>
        /// <param name="count">增加数量，默认1个</param>
        public void AddItems(int count = 1)
        {
            _Rows += count;
            _Handle = _Rows - 1;

            var page = _Current;
            Refresh();

            if (_Current > page)
            {
                // 切换了页码需要重新加载数据
                CurrentPageChanged?.Invoke(this, new PageControlEventArgs(FocusedRowHandle));
            }
            else
            {
                TotalRowsChanged?.Invoke(this, new PageControlEventArgs(FocusedRowHandle));
            }
        }

        /// <summary>
        /// 减少列表成员
        /// </summary>
        /// <param name="count">减少数量，默认1个</param>
        public void RemoveItems(int count = 1)
        {
            _Rows -= count;
            if (_Handle >= _Rows) _Handle = _Rows - 1;

            var page = _Current;
            Refresh();

            if ((_Rows > 0 && _Handle < _PageSize*(_TotalPages - 1)) || _Current < page)
            {
                // 不是末页或切换了页码需要重新加载数据
                CurrentPageChanged?.Invoke(this, new PageControlEventArgs(FocusedRowHandle));
            }
            else
            {
                TotalRowsChanged?.Invoke(this, new PageControlEventArgs(FocusedRowHandle));
            }
        }

        /// <summary>
        /// 切换每页行数
        /// </summary>
        private void PageRowsChanged()
        {
            var handel = FocusedRowHandle;
            var change = _PageSize < _Rows - _PageSize*_Current;
            _PageSize = int.Parse(cbeRows.Text);
            PageSizeChanged?.Invoke(this, new PageSizeEventArgs(_PageSize));

            var page = _Current;
            Refresh();

            change = change || _PageSize < _Rows - _PageSize*_Current;
            if (!change && _Current == page && FocusedRowHandle == handel) return;

            // 切换了页码或当前页显示行数变化后需要重新加载数据
            CurrentPageChanged?.Invoke(this, new PageControlEventArgs(FocusedRowHandle));
        }

        /// <summary>
        /// 切换当前页
        /// </summary>
        /// <param name="page">页码</param>
        private void ChangePage(int page)
        {
            _Handle = _PageSize*page;

            Refresh();

            CurrentPageChanged?.Invoke(this, new PageControlEventArgs(FocusedRowHandle));
        }

        /// <summary>
        /// 刷新控件
        /// </summary>
        private new void Refresh()
        {
            if (_Handle > _Rows) _Handle = 0;

            var total = _TotalPages == 0 ? 1 : _TotalPages;
            labRows.Text = $" 行/页 | 共 {_Rows} 行 | 分 {total} 页";
            labRows.Refresh();

            var val = (int) Math.Floor((decimal) _Handle/_PageSize);
            _Current = val < 0 ? 0 : val;
            btnFirst.Enabled = _Current > 0;
            btnPrev.Enabled = _Current > 0;
            btnNext.Enabled = _Current < _TotalPages - 1;
            btnLast.Enabled = _Current < _TotalPages - 1;
            btnJump.Enabled = _TotalPages > 1;

            var width = (int) Math.Log10(_Current + 1)*7 + 18;
            btnJump.Width = width;
            btnJump.Text = CurrentPage.ToString();
            labRows.Focus();
        }

        /// <summary>
        /// 跳转到指定页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Jump_Click(object sender, EventArgs e)
        {
            txtPage.Visible = true;
            txtPage.Focus();
        }

        /// <summary>
        /// 焦点离开输入框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageInput_Leave(object sender, EventArgs e)
        {
            txtPage.EditValue = null;
            txtPage.Visible = false;
        }

        /// <summary>
        /// 输入页码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)
            {
                txtPage.EditValue = null;
                txtPage.Visible = false;
                return;
            }

            if (e.KeyChar != 13) return;

            if (string.IsNullOrEmpty(txtPage.Text)) return;

            var page = int.Parse(txtPage.Text);
            if (page < 1 || page > _TotalPages || page == _Current + 1)
            {
                txtPage.EditValue = null;
                return;
            }

            txtPage.Visible = false;
            ChangePage(page - 1);
        }
    }

    public class PageSizeEventArgs : EventArgs
    {
        /// <summary>
        /// PageSize
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rows">RowsPerPage</param>
        public PageSizeEventArgs(int rows)
        {
            PageSize = rows;
        }
    }

    public class PageControlEventArgs : EventArgs
    {
        /// <summary>
        /// RowHandle
        /// </summary>
        public int RowHandle { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="handel">RowHandle</param>
        public PageControlEventArgs(int handel)
        {
            RowHandle = handel;
        }
    }
}