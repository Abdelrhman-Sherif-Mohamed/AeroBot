using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SDUI.Controls
{
    public class WindowPageControl : UserControl
    {
        private EventHandler<int> _onSelectedIndexChanged;

        public event EventHandler<int> SelectedIndexChanged
        {
            add => _onSelectedIndexChanged += value;
            remove => _onSelectedIndexChanged -= value;
        }

        /// <summary>
        ///     Raised whenever the visual order (or the set) of pages changes.
        /// </summary>
        public event EventHandler PagesChanged;

        /// <summary>
        ///     Pages in their stable visual order. This list is intentionally independent
        ///     from the WinForms z-order, so pages can never jump around the sidebar when
        ///     controls are brought to front, hidden or re-added.
        /// </summary>
        private readonly List<Control> _pages = new();

        private int _selectedIndex = -1;
        public int SelectedIndex 
        { 
            get => _selectedIndex; 
            set 
            {
                var sys = Stopwatch.StartNew();

                if (_selectedIndex == value)
                    return;

                if (_pages.Count > 0)
                {
                    if (value < 0)
                        value = _pages.Count - 1;

                    if (value > _pages.Count - 1)
                        value = 0;
                }
                else
                    value = -1;

                var previousSelectedIndex = _selectedIndex;
                _selectedIndex = value;
                _onSelectedIndexChanged?.Invoke(this, previousSelectedIndex);

                SuspendLayout();
                try
                {
                    for (int i = 0; i < _pages.Count; i++)
                        _pages[i].Visible = i == _selectedIndex;
                }
                finally
                {
                    ResumeLayout(true);
                }

                Debug.WriteLine($"Index: {_selectedIndex} Finished: {sys.ElapsedMilliseconds} ms");
            } 
        }

        public int Count => _pages.Count;

        /// <summary>
        ///     Returns the page at the given visual index.
        /// </summary>
        public Control GetPage(int index)
        {
            if (index < 0 || index >= _pages.Count)
                return null;

            return _pages[index];
        }

        /// <summary>
        ///     Returns the visual index of the given page, or -1 when it is not tracked.
        /// </summary>
        public int IndexOfPage(Control control)
        {
            if (control == null)
                return -1;

            return _pages.IndexOf(control);
        }

        /// <summary>
        ///     Moves an already tracked page to the given visual position.
        /// </summary>
        public void MovePageTo(Control control, int index)
        {
            var oldIndex = _pages.IndexOf(control);
            if (oldIndex < 0)
                return;

            if (index < 0)
                index = 0;
            if (index > _pages.Count - 1)
                index = _pages.Count - 1;

            if (oldIndex == index)
                return;

            _pages.RemoveAt(oldIndex);
            _pages.Insert(index, control);

            PagesChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        /// <summary>
        ///     Applies an explicit visual order to the tracked pages. Pages that are not
        ///     part of the given sequence keep their relative order at the end.
        /// </summary>
        public void SetPageOrder(IEnumerable<Control> orderedPages)
        {
            if (orderedPages == null)
                return;

            var ordered = new List<Control>(_pages.Count);

            foreach (var page in orderedPages)
            {
                if (page != null && _pages.Contains(page) && !ordered.Contains(page))
                    ordered.Add(page);
            }

            foreach (var page in _pages)
            {
                if (!ordered.Contains(page))
                    ordered.Add(page);
            }

            bool changed = ordered.Count != _pages.Count;
            if (!changed)
            {
                for (int i = 0; i < ordered.Count; i++)
                {
                    if (ordered[i] != _pages[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (!changed)
                return;

            _pages.Clear();
            _pages.AddRange(ordered);

            PagesChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        public WindowPageControl() 
        {
            DoubleBuffered = true;
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer, true
            );

            //BackColor = Color.Transparent;
            UpdateStyles();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            e.Control.Dock = DockStyle.Fill;
            e.Control.BackColor = Color.Transparent;

            if (!_pages.Contains(e.Control))
                _pages.Add(e.Control);

            e.Control.Visible = _pages.Count == 1;

            if (_pages.Count == 1)
                _selectedIndex = 0;

            PagesChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            base.OnControlRemoved(e);

            _pages.Remove(e.Control);

            if (_pages.Count == 0)
            {
                _selectedIndex = -1;
            }
            else if (_selectedIndex >= _pages.Count)
            {
                _selectedIndex = _pages.Count - 1;
            }

            for (int i = 0; i < _pages.Count; i++)
                _pages[i].Visible = i == _selectedIndex;

            PagesChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.ExStyle |= 0x02000000;
                return parms;
            }
        }
    }
}
