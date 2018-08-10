import * as React from 'react';
import { FieldType } from "../States/CostEditorStates";
import { EditItem } from "../States/CostBlockStates";
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField, Dialog} from '@extjs/ext-react';
import { NamedId } from '../../Common/States/CommonStates';
import { HistoryValuesGridContainer } from './HistoryValuesGridContainer';

Ext.require([
    'Ext.grid.plugin.CellEditing', 
    'Ext.panel.Resizer'
]);

export interface ValueColumnProps {
    title: string
    type: FieldType,
    selectedItems: NamedId<number>[]
}

export interface EditGridActions {
    onItemEdited?(item: EditItem)
    onApplyFilters?()
    onCleared?()
    onSaving?(forApproval: boolean)
}

export interface EditGridProps extends EditGridActions {
    nameColumnTitle: string
    valueColumn: ValueColumnProps
    items: EditItem[]
    isEnableSave: boolean
    isEnableClear: boolean
    isEnableApplyFilters: boolean
    flex?: number
}

export interface EditGridState {
    selectedEditItem: EditItem
    isVisibleHistoryWindow: boolean
}

export class EditGrid extends React.Component<EditGridProps, EditGridState> {
    constructor(props: EditGridProps) {
        super(props);

        this.state = {
            selectedEditItem: null,
            isVisibleHistoryWindow: false
        }
    }

    public render() {
        const props = this.props;

        const store = Ext.create('Ext.data.Store', {
            data: props.items && props.items.map(item => ({
                ...item, 
                value: item.valueCount == 1 ? item.value : 0
            })),
            listeners: props.onItemEdited && {
                update: ((store, record, operation, modifiedFieldNames, details) => {
                    if (modifiedFieldNames[0] === 'name') {
                        record.reject();
                    } else {
                        //HACK: Need for displaying new value.
                        record.set('valueCount', 1);
    
                        props.onItemEdited(record.data);
                    }
                })
            }
        });

        return (
            <Grid 
                store={store} 
                flex={props.flex} 
                shadow 
                columnLines={true}
                plugins={['cellediting', 'selectionreplicator']}
                // selectable={{
                //     rows: true,
                //     cells: true,
                //     columns: true,
                //     drag: true,
                //     extensible: 'y'
                // }}
                onSelect={this.onSelectGrid}
            >
                <Toolbar docked="top">
                    <Button 
                        text="Apply filters" 
                        flex={1} 
                        disabled={!props.isEnableApplyFilters}
                        handler={props.onApplyFilters}
                    />

                    <Button 
                        text="History" 
                        flex={1} 
                        disabled={!this.state.selectedEditItem}
                        handler={this.showHistoryWindow}
                    />
                </Toolbar>
                
                <Column text={props.nameColumnTitle} dataIndex="name" flex={1} extensible={false} />
                {this.getValueColumn(props.valueColumn)}
        
                <Toolbar docked="bottom">
                    <Button 
                        text="Clear" 
                        flex={1} 
                        disabled={!props.isEnableClear}
                        handler={() => this.showClearDialog()}
                    />
                    <Button 
                        text="Save" 
                        flex={1} 
                        disabled={!props.isEnableSave}
                        handler={() => this.showSaveDialog(false)}
                    />
                    <Button 
                        text="Save and send for approval" 
                        flex={1} 
                        disabled={!props.isEnableSave}
                        handler={() => this.showSaveDialog(true)}
                    />
                </Toolbar>

                { this.getHistoryWindow() }
            </Grid>
        );
    }

    private showSaveDialog(forApproval: boolean) {
        const { onSaving } = this.props;
    
        Ext.Msg.confirm(
          'Saving changes', 
          'Do you want to save the changes?',
          (buttonId: string) => onSaving && onSaving(forApproval)
        );
    }
    
    private showClearDialog() {
        const { onCleared } = this.props;

        Ext.Msg.confirm(
            'Clearing changes', 
            'Do you want to clear the changes??',
            (buttonId: string) => onCleared && onCleared()
        );
    }

    private getValueColumn(columProps: ValueColumnProps) {
        let columnOptions;
        let renderer: (value, data: { data: EditItem }) => string;
    
        switch (columProps.type) {
            case FieldType.Reference:
                var options = columProps.selectedItems.map(item => ({text: item.name, value: item.id}));
    
                columnOptions = (
                    <SelectField options={options}/>
                );

                renderer = (value, { data }) => {
                    let result: string;

                    if (data.valueCount == 1) {
                        const selectedItem = columProps.selectedItems.find(item => item.id == data.value);

                        result = selectedItem.name;
                    } else {
                        result = this.getValueCountMessage(data);
                    }

                    return result;
                }
                break;

            case FieldType.Double:
                renderer = (value, { data }) => data.valueCount == 1 ? value : this.getValueCountMessage(data);
                break;
        }
    
        return (
            <Column 
                text={columProps.title} 
                dataIndex="value" 
                flex={1} 
                editable={true}
                renderer={renderer}
            >
                {columnOptions}
            </Column>
        )
    }

    private getValueCountMessage(editItem: EditItem) {
        return `(${editItem.valueCount} values)`
    }

    private getHistoryWindow() {
        const { isVisibleHistoryWindow, selectedEditItem} = this.state;

        return (
            isVisibleHistoryWindow && selectedEditItem &&
            <Dialog 
                displayed={isVisibleHistoryWindow} 
                title="History" 
                closable 
                maximizable
                resizable={{
                    dynamic: true,
                    edges: 'all'
                }}
                minHeight="500"
                minWidth="400"
                onHide={this.hideHistoryWindow}
            >
                <HistoryValuesGridContainer editItemId={selectedEditItem.id} />
            </Dialog>
        );
    }

    private showHistoryWindow = () => {
        this.setState({
            isVisibleHistoryWindow: true
        });
    }

    private hideHistoryWindow = () => {
        this.setState({ 
            isVisibleHistoryWindow: false 
        });
    }

    private onSelectGrid = (grid, records: { data: EditItem }[]) => {
        const editItem = records.length > 0 
            ? records[0].data 
            : null;

        this.setState({
            selectedEditItem: editItem
        });
    }
}