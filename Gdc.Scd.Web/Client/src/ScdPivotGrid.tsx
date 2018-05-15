import * as React from 'react';
import { PivotGrid } from '@extjs/ext-react-pivot'
import { Toolbar, Button, Menu, MenuItem, Container } from '@extjs/ext-react';
import EditCellsWindow from './EditCellsWindow';

interface ScdPivotGridState {
    isVisibleEditDialog: boolean;
}

class AggregateValueInfo {
    value: number;
    type: AggregateType;
}

enum AggregateType {
    WithoutAggregation,
    InstallBaseValue,
    InstallBaseAggregate
}

export class ScdPivotGrid extends React.Component<any, ScdPivotGridState> {
  private pivotgrid: PivotGrid & any;
  private installBase: {[key: string]: number};
  private matrix;
  private selection: any;
  private selectedRecords: { data: {[key: string]: any}}[];
  private configPlugin;
  private exportPlugin;
  private drillDownPlugin;
  private rangeEditorPlugin;

  public componentWillMount() {
      const data = this.getData();
      const store = this.getStore(data.data);

      this.installBase = data.instalBase;
      this.matrix = this.getMatrix(store);
      this.setState({isVisibleEditDialog: false})
  }

  public render() {
    return (
        <Container fullscreen layout="fit">
            <PivotGrid 
                ref={pivotgrid => this.pivotgrid = pivotgrid}
                matrix={this.matrix} 
                store={null}
                plugins={[
                    this.getConfigurablePlugin(), 
                    this.getExportPlugin(), 
                    this.getDrillDownPlugin(), 
                    // this.getRangeEditorPluging()
                ]}
                selectable={{
                    mode: 'multi',
                    drag: true,
                    columns: true,
                    cells: true,
                    rows: true,
                    extensible: true,
                    // checkbox: true
                }}
                onSelectionChange={this.onSelectionChange.bind(this)}
            >
                <Toolbar docked="top" layout={{ type: 'hbox', align: 'stretch'}}>
                    <Button text="Configurator" handler={() => this.pivotgrid.showConfigurator()} />
                    <Button text="Export to ...">
                        <Menu defaults={{ handler: menuItem => this.export(menuItem)}}>
                            <MenuItem text="Excel" cfg={{type: 'excel07', fileName: 'Pivot.xlsx', onlyExpandedNodes: true}} />
                        </Menu>
                    </Button>
                    <Button text="Edit" handler={() => this.onEditClick()} />
                </Toolbar>
            </PivotGrid>

            <EditCellsWindow 
                isVisible={this.state.isVisibleEditDialog} 
                onEditClick={this.onEditCellDialogClick}
                onCancelClick={this.onCancelDialogClick}
            /> 
        </Container>
    );
  }

  private onEditCellDialogClick = (value: number) => {
    if (this.selection.startCell && this.selection.endCell) {
        this.updateValues(
            this.pivotgrid, 
            this.selectedRecords, 
            this.selection.startCell.columnIndex, 
            this.selection.endCell.columnIndex, 
            value);
    }

    this.setState({ 
        ...this.state, 
        isVisibleEditDialog: false
    });
  }

  private onCancelDialogClick = () => {
    this.setState({ 
        ...this.state, 
        isVisibleEditDialog: false
    });
  }

  private onSelectionChange = (view: any, records: any, selected: boolean, selection: any) => {
    this.selectedRecords = records;
    this.selection = selection;
  }

  private onEditClick = () => {
    this.setState({ 
        ...this.state, 
        isVisibleEditDialog: true
    });
  }

  private updateValues(
      pivotgrid: PivotGrid & any, 
      pivotRecords: { data: {[key: string]: any}}[], 
      startColumnIndex: number, 
      endColumnIndex: number,
      value: number) {
    const matrix = pivotgrid.getMatrix();
    const matrixColumns: {[key: string]: any}[] = matrix.getColumns();
    const pivotColumns: {[key: string]: any}[] = pivotgrid.getColumns();
    const columnInfos = 
        pivotColumns.filter((column, index) => startColumnIndex <= index && index <= endColumnIndex)
                    .map(pivotColumn => ({ 
                        dataIndex: pivotColumn.dimension.dataIndex,
                        topAxisKey: matrixColumns.find(matrixColumn => matrixColumn.name === pivotColumn.getDataIndex()).col
                    }));

    for (const record of pivotRecords) {
        for (const columnInfo of columnInfos) {
            const updater = new (Ext as any).pivot.update.Overwrite({
                leftKey: record.data.leftAxisKey,
                topKey: columnInfo.topAxisKey,
                dataIndex: columnInfo.dataIndex,
                matrix,
                value
            });

            updater.update();
        }
    }
  }

  private getLeftAxisConfig() {
      return [
        {
            dataIndex: 'serviceOfferedGroup',
            header: 'Service Offered Group',
            width: 200
        },
        {
            dataIndex: 'warrantyGroup',
            header: 'Warranty Group',
            width: 200
        },
    ];
  }

  private getTopAxisConfig() {
    return [
        {
            dataIndex: 'costBlock',
            header: 'Cost Block',
            width: 200
        },
        {
            dataIndex: 'costElement',
            header: 'Cost Element',
            width: 200
        }
    ];
  }

  private getMatrix(store) {
    return Ext.create('Ext.pivot.matrix.Local', {
        store: store,
        leftAxis: this.getLeftAxisConfig(),
        topAxis: this.getTopAxisConfig(),
        aggregate: [
            {
                dataIndex: 'costElementValue',
                header: 'Installation Base',
                aggregator: this.aggregateInstalationBase.bind(this),
                renderer: this.aggregateRenderer.bind(this)
            }
        ]
    });
  }

  private aggregateInstalationBase(records: { data: {[key: string]: any}}[], measure: string, matrix, rowGroupKey: string, colGroupKey: string) {
    let result = new AggregateValueInfo();

    const firstRecord = records[0];

    if (records.length === 1) {
        result.value = firstRecord.data[measure];
        result.type = AggregateType.WithoutAggregation;
    } else {
        const warrantyGroup = firstRecord.data['warrantyGroup'];
        const serviceOfferedGroup = firstRecord.data['serviceOfferedGroup'];

        const isSameGroups = Ext.Array.every(
            records, 
            record => 
                record.data['warrantyGroup'] === warrantyGroup && 
                record.data['serviceOfferedGroup'] === serviceOfferedGroup);

        if (isSameGroups) {
            result.value = this.installBase[this.getInstallBaseKeyByRecord(firstRecord)];
            result.type = AggregateType.InstallBaseValue;
        } else {
            result.value = this.computeInstallBaseAgregation(records);
            result.type = AggregateType.InstallBaseAggregate;
        }
    }

    return result;
  }

  private aggregateRenderer(value: AggregateValueInfo, record, dataIndex, cell, column) {
    let background

    switch (value.type) {
        case AggregateType.InstallBaseValue: 
            cell.setStyle({ background: '#8080802e' });
            break;
        
        case AggregateType.WithoutAggregation: 
            cell.setStyle({ background: '#01d2012b' });
            break;
            
        default:
            cell.setStyle({ background: null });
            break;
    }
    
    return Ext.util.Format.number(value.value, '0,000.00');
  }

  private computeInstallBaseAgregation(records: { data: {[key: string]: any}}[]) {
    let result: number;

    const installBaseKeys = records.map(record => this.getInstallBaseKeyByRecord(record));
    const installBaseKeySet = new Set(installBaseKeys);
    const installBaseSum = 
        Array.from(installBaseKeySet)
             .reduce<number>((prevVal, curVal) => prevVal + this.installBase[curVal], 0);

    result = 
        records.reduce<number>(
            (prevVal, curVal) => 
                prevVal + curVal.data['costElementValue']/this.installBase[this.getInstallBaseKeyByRecord(curVal)], 
            0
        );
    
    result = result / records.length * installBaseSum;

    return result;
  }

  private getStore(data: {}[]) {
    return Ext.create('Ext.data.Store', {
        fields: [
            'serviceOfferedGroup',
            'warrantyGroup',
            'costBlock',
            'costElement',
            'costElementValue',
        ],
        data: data
    });
  }

  private getData() {
    const data = [];
    const instalBase = {};

    for (let sogIndex = 1; sogIndex < 5; sogIndex++) {
        for (let wgIndex = 1; wgIndex < 5; wgIndex++) {
            for (let cbIndex = 1; cbIndex < 5; cbIndex++) {
                for (let ceIndex = 1; ceIndex < 5; ceIndex++) {
                    const serviceOfferedGroup = `Service Offered Group ${sogIndex}`;
                    const warrantyGroup = `Warranty Group ${wgIndex}`;

                    data.push({
                        serviceOfferedGroup,
                        warrantyGroup,
                        costBlock: `Cost Block ${cbIndex}`,
                        costElement: `Cost Element ${ceIndex}`,
                        costElementValue: (1000 + sogIndex + wgIndex + cbIndex) * ceIndex,
                    });

                    instalBase[this.getInstallBaseKey(serviceOfferedGroup, warrantyGroup)] = (sogIndex + wgIndex) * 70;
                }
            }
        }
    }

    console.log(`Row count: ${data.length}`);
    console.log('Instal base:');
    console.log(instalBase);

    return { data, instalBase };
  }

  private getInstallBaseKey(serviceOfferedGroup: string, warrantyGroup: string) {
    return `${serviceOfferedGroup}_${warrantyGroup}`;
  }

  private getInstallBaseKeyByRecord(record: { data: {[key: string]: any}}) {
    return this.getInstallBaseKey(record.data['serviceOfferedGroup'], record.data['warrantyGroup']);;
  }

  private getConfigurablePlugin() {
    if (!this.configPlugin) {
        this.configPlugin = Ext.create('Ext.pivot.plugin.Configurator', {
            panelWrap: false,
            fields: [
                {
                    dataIndex: 'serviceOfferedGroup',
                    header: 'Service Offered Group',
                },
                {
                    dataIndex: 'warrantyGroup',
                    header: 'Warranty Group',
                },
                {
                    dataIndex: 'costBlock',
                    header: 'Cost Block',
                },
                {
                    dataIndex: 'costElement',
                    header: 'Cost Element',
                }
            ]
        });
    }

    return this.configPlugin;
  }

  private getExportPlugin() {
    if (!this.exportPlugin) {
        this.exportPlugin = Ext.create('Ext.pivot.plugin.Exporter', { }); 
    }

    return this.exportPlugin;
  }
  
  private getDrillDownPlugin() {
    if (!this.drillDownPlugin) {
        this.drillDownPlugin = Ext.create('Ext.pivot.plugin.DrillDown', { }); 
    }

    return this.drillDownPlugin;
  }

  private getRangeEditorPluging() {
    if (!this.rangeEditorPlugin) {
        this.rangeEditorPlugin = Ext.create('Ext.pivot.plugin.RangeEditor', { }); 
    }

    return this.rangeEditorPlugin;
  }

  private export({cfg}){
    cfg = { ...cfg, title: 'Pivot grid'};

    this.pivotgrid.saveDocumentAs(cfg);
  }
}