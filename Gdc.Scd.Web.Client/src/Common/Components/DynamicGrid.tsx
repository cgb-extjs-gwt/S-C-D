import * as React from "react";
import {
  Grid,
  Column,
  CheckColumn,
  NumberField,
  TextField,
  SelectField,
  Toolbar,
  Button,
  Container
} from "@extjs/ext-react";
import {
  ColumnInfo,
  ColumnType,
  FilterItem,
  ColumnFilter
} from "../States/ColumnInfo";
import { SaveToolbar } from "./SaveToolbar";
import { Model, StoreOperation, Store } from "../States/ExtStates";
import { ReactNode } from "react-redux";
import {
  DynamicGridProps,
  ToolbarDynamicGridProps
} from "./Props/DynamicGridProps";
import { objectPropsEqual } from "../Helpers/CommonHelpers";
import { buildReferenceColumnRendered } from "../Helpers/GridHeper";

export interface StoreDynamicGridProps extends ToolbarDynamicGridProps {
  store: Store;
  useStoreSync?: boolean;
}

export class DynamicGrid extends React.PureComponent<StoreDynamicGridProps> {
  private saveToolbar: SaveToolbar;
  private columnsMap = new Map<string, ColumnInfo>();
  private store: Store;
  private columns: ColumnInfo[];

  public componentDidMount() {
    const { init, store, columns } = this.props;

    this.store = store;
    this.columns = columns;

    init && init();
  }

  public componentWillUnmount() {
    this.removeStoreListeners();
  }

  public render() {
    this.init();

    const {
      id,
      flex,
      getSaveToolbar = this.getSaveToolbar,
      height,
      width,
      minWidth,
      minHeight,
      children
    } = this.props;
    const isEditable =
      this.columns && !!this.columns.find(column => column.isEditable);

    const gridProps = isEditable
      ? {
          plugins: ["cellediting", "selectionreplicator"],
          selectable: {
            rows: true,
            cells: true,
            columns: false,
            drag: true,
            extensible: "y"
          }
        }
      : {};

    const hasChanges = this.hasChanges();

    return this.store ? (
      <Grid
        {...gridProps}
        flex={flex}
        store={this.store}
        columnLines={true}
        height={height}
        width={width}
        minHeight={minHeight}
        minWidth={minWidth}
        onSelectionchange={this.onSelectionChange}
        onColumnMenuCreated={this.onColumnMenuCreated}
      >
        {this.columns &&
          this.columns
            .filter(column => !column.isInvisible)
            .map(column => this.buildColumn(id, column))}
        {children}
        {isEditable && getSaveToolbar(hasChanges, this.toolbarRef, this)}
      </Grid>
    ) : (
      <div />
    );
  }

  public cancel = () => {
    const { onCancel } = this.props;

    this.store.rejectChanges();

    onCancel && onCancel();
  };

  public saveWithCallback = (callback: () => void) => {
    const { useStoreSync } = this.props;
    const save = () => {
      callback && callback();

      this.saveToolbar.enable(this.hasChanges());
    };

    if (useStoreSync) {
      this.store.sync({
        callback: save
      });
    } else {
      this.store.commitChanges();
      save();
    }
  };

  public save = () => {
    this.saveWithCallback(this.props.onSave);
  };

  private init() {
    const { store, columns } = this.props;

    if (this.store != store) {
      this.removeStoreListeners();
      this.addStoreListeners(store);

      this.store = store;
    }

    if (this.columns != columns) {
      this.columnsMap.clear();

      if (columns) {
        this.runByEachColumn(columns, column =>
          this.columnsMap.set(column.dataIndex, column)
        );

        this.columns = columns;
      } else {
        this.columns = [];
      }
    }
  }

  private runByEachColumn(columns: ColumnInfo[], fn: (column) => void) {
    for (const column of columns) {
      fn(column);

      if (column.columns) {
        this.runByEachColumn(column.columns, fn);
      }
    }
  }

  private onSelectionChange = (
    grid,
    records: Model[],
    selecting: boolean,
    selectionInfo
  ) => {
    const { onSelectionChange } = this.props;

    onSelectionChange &&
      onSelectionChange(grid, records, selecting, selectionInfo);
  };

  private getSaveToolbar = (
    hasChanges: boolean,
    ref: (toolbar: SaveToolbar) => void,
    grid: DynamicGrid
  ) => {
    return (
      <SaveToolbar
        ref={ref}
        isEnableClear={hasChanges}
        isEnableSave={hasChanges}
        onCancel={this.cancel}
        onSave={this.save}
      />
    );
  };

  private toolbarRef = (toolbar: SaveToolbar) => {
    this.saveToolbar = toolbar;
  };

  private hasChanges() {
    return this.store ? this.store.getModifiedRecords().length > 0 : false;
  }

  private buildColumn(gridId: string, column: ColumnInfo) {
    const columnOption = this.buildColumnOption(gridId, column);

    switch (column.type) {
      case ColumnType.CheckBox:
        return <CheckColumn {...columnOption} />;

      default:
        let editor = null;

        if (column.isEditable) {
          switch (column.type) {
            case ColumnType.Reference:
              editor =
                column.referenceItems.size == 0 ? null : (
                  <SelectField
                    options={this.getReferenceEditorOptions(column)}
                  />
                );
              break;

            case ColumnType.Numeric:
              editor = (
                <NumberField
                  required
                  validators={{ type: "number", message: "Invalid value" }}
                />
              );
              break;

            case ColumnType.Text:
              editor = <TextField />;
              break;
          }
        }

        return <Column {...columnOption}>{editor}</Column>;
    }
  }

  private buildColumnOption(gridId: string, column: ColumnInfo) {
    const columnOption: any = {
      key: `${gridId}_${column.dataIndex}`,
      dataIndex: column.dataIndex,
      editable: column.isEditable,
      text: column.title,
      width: column.width
    };

    //TODO: Breaks editing
    if (column.dataIndex != null) {
      columnOption.itemId = column.dataIndex.replace(/\./g, "");
    }

    if (column.flex) {
      columnOption.flex = column.flex;
    }

    if (column.rendererFn) {
      columnOption.renderer = (value, record: Model, dataIndex: string, cell) =>
        this.replaceNullValue(column.rendererFn(value, record, dataIndex, cell));
    } else {
      columnOption.renderer = this.replaceNullValue;
    }

    if (column.filter) {
      columnOption.menu = ["-", this.buildColumnSearchField(column.filter)];
    }

    if (column.extensible != null) {
      columnOption.extensible = column.extensible;
    }

    if (column.columns) {
      columnOption.columns = column.columns.map(col =>
        this.buildInnerColumn(gridId, col)
      );
    }

    switch (column.type) {
      case ColumnType.CheckBox:
        columnOption.disabled = !column.isEditable;
        columnOption.headerCheckbox = column.isEditable;
        break;

      default:
        let editor = null;

        if (column.isEditable) {
          switch (column.type) {
            case ColumnType.Reference:
              columnOption.renderer = buildReferenceColumnRendered(column);
              break;
          }
        }
        break;
    }

    return columnOption;
  }

  private buildInnerColumn(gridId: string, column: ColumnInfo) {
    const columnOption = this.buildColumnOption(gridId, column);

    switch (column.type) {
      case ColumnType.CheckBox:
        columnOption.xtype = "checkcolumn";
        break;

      default:
        if (column.isEditable) {
          switch (column.type) {
            case ColumnType.Reference:
              columnOption.editor = {
                xtype: "selectfield",
                options: this.getReferenceEditorOptions(column)
              };
              break;

            case ColumnType.Numeric:
              columnOption.editor = {
                xtype: "numberfield",
                required: true,
                validators: {
                  type: "number",
                  message: "Invalid value"
                }
              };
              break;

            case ColumnType.Text:
              columnOption.editor = {
                xtype: "textfield"
              };
              break;
          }
        }
        break;
    }

    return columnOption;
  }

  private getReferenceEditorOptions(column: ColumnInfo) {
    return Array.from(column.referenceItems.values()).map(item => ({
      text: item.name,
      value: item.id
    }));
  }

  private onColumnMenuCreated = (grid, column, menu) => {
    const dataIndex = column.getDataIndex();
    const columnInfo = this.columnsMap.get(dataIndex);
    const filterGridId = "filterGrid";

    if (columnInfo.filter) {
      menu.on("beforeshow", () => {
        menu.add(this.buildColumnFilterGrid(columnInfo.filter, filterGridId));
      });

      menu.on("hide", () => {
        menu.remove(menu.queryById(filterGridId));
      });
    }
  };

  private buildColumnSearchField(filter: ColumnFilter) {
    let searchFn = null;

    return {
      xtype: "textfield",
      label: "Search",
      margin: "-15 10 10 10",
      listeners: {
        change: (field, newValue: string, oldValue: string) => {
          const filters = filter.store.getFilters();

          if (searchFn) {
            filters.remove(searchFn);
          }

          searchFn = (item: Model<FilterItem>) => {
            let valueStr: string = item.data.value.toString();
            valueStr = valueStr.toLowerCase();

            return valueStr.includes(newValue.toLowerCase());
          };

          filters.add(searchFn);
        }
      }
    };
  }

  private buildColumnFilterGrid(filter: ColumnFilter, id) {
    return {
      xtype: "grid",
      id,
      columnLines: true,
      rowLines: true,
      store: filter.store,
      height: 450,
      margin: "-5 10 10 10",
      style: "border: 1px solid rgb(226, 226, 226);",
      columns: [
        {
          xtype: "checkcolumn",
          dataIndex: filter.checkedDataIndex,
          width: 70,
          sortable: false
        },
        {
          text: "Value",
          dataIndex: filter.valueDataIndex,
          width: 200,
          renderer: this.replaceNullValue
        }
      ]
    };
  }

  private replaceNullValue = value => (value ? value : " ");

  private onUpdateStore = (
    store: Store,
    record: Model,
    operation: StoreOperation,
    modifiedFieldNames: string[],
    details
  ) => {
    switch (operation) {
      case StoreOperation.Edit:
        if (this.saveToolbar) {
          this.saveToolbar.enable(true);
        }

        modifiedFieldNames.forEach(dataIndex => {
          const column = this.columnsMap.get(dataIndex);

          column &&
            column.editMappingFn &&
            column.editMappingFn(record, dataIndex);
        });
        break;

      case StoreOperation.Reject:
      case StoreOperation.Commit:
        if (this.saveToolbar) {
          this.saveToolbar.enable(false);
        }
        break;
    }
  };

  private addStoreListeners = (store: Store) => {
    if (store) {
      store.on("update", this.onUpdateStore, this);
    }
  };

  private removeStoreListeners = () => {
    this.store && this.store.un("update", this.onUpdateStore);
  };
}
