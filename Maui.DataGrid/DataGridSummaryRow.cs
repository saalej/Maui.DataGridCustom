
using Maui.DataGrid;
using Maui.DataGrid.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Maui.DataGrid
{
    internal sealed class DataGridSummaryRow : Grid
    {
        #region Bindable Properties
        public static readonly BindableProperty DataGridProperty =
             BindablePropertyExtensions.Create<DataGridSummaryRow, DataGrid>(null, BindingMode.OneTime);
    
        #endregion Bindable Properties
        
        #region Fields
        public DataGrid DataGrid
        {
            get => (DataGrid)GetValue(DataGridProperty);
            set => SetValue(DataGridProperty, value);
        }
    
        private readonly RowDefinitionCollection _summaryRowDefinitions =
        [
            new() { Height = new(1, GridUnitType.Star) }
        ];
        #endregion Fields
    
        #region Methods
        internal void InitializeSummaryRow(bool force = false)
        {
            if (DataGrid == null) return;
            if (!DataGrid.IsLoaded && !force) return;
    
            Children.Clear();
    
            if (DataGrid.Columns == null || DataGrid.Columns.Count == 0)
            {
                ColumnDefinitions.Clear();
                return;
            }
    
            for (var i = 0; i < DataGrid.Columns.Count; i++)
            {
                var col = DataGrid.Columns[i];
    
                col.DataGrid ??= DataGrid;
                col.BindingContext ??= BindingContext;
    
                ColumnDefinitions.AddOrUpdate(col.ColumnDefinition, i);
    
                col.SummaryCell = CreateSummaryCell(col);
                col.SummaryCell.UpdateBindings(DataGrid);
    
                if (!col.IsVisible)
                {
                    continue;
                }
    
                if (Children.TryGetItem(i, out var existingChild))
                {
                    if (existingChild is not DataGridCell existingCell)
                    {
                        throw new InvalidDataException($"Header row should only contain {nameof(DataGridCell)}s");
                    }
    
                    if (existingCell.Column != col)
                    {
                        this.SetColumn(col.SummaryCell, i);
                        Children[i] = col.SummaryCell;
                    }
                }
                else
                {
                    this.SetColumn(col.SummaryCell, i);
                    Children.Add(col.SummaryCell);
                }
            }
            ColumnDefinitions.RemoveAfter(DataGrid.Columns.Count);
        }
    
        private DataGridCell CreateSummaryCell(DataGridColumn column)
        {
            if (column.SummaryCell != null)
            {
                return column.SummaryCell;
            }
    
            var cellContent = new Grid
            {
                RowDefinitions = _summaryRowDefinitions,
            };
    
            column.SummaryLabel.Style = column.SummaryLabelStyle ?? DataGrid.SummaryLabelStyle ?? DataGrid.DefaultSummaryLabelStyle;

            if (column.ShowSum)
            {
                column.SummaryLabel.SetBinding(Label.TextProperty, new Binding(source: column,
                                                                           path: nameof(DataGridColumn.SumValue),
                                                                           stringFormat: "{0:N2}"));
            }
            else
            {
                column.SummaryLabel.Text = string.Empty;
                column.SummaryLabel.RemoveBinding(Label.TextProperty); // Opcional: elimina el binding anterior
            }
            
            column.SummaryLabelContainer.Children.Add(column.SummaryLabel);
    
            cellContent.Children.Add(column.SummaryLabelContainer);
    
            return new DataGridCell(cellContent, DataGrid.SummaryRowBackground, column, false);
        }
    
        private void OnColumnsChanged(object? sender, EventArgs e)
        {
            InitializeSummaryRow();
        }
    
        private void OnVisibilityChanged(object? sender, EventArgs e)
        {
            InitializeSummaryRow();
        }
    
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            InitializeSummaryRow();
        }
        #endregion Methods
    }
}
