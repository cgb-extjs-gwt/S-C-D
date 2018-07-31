import * as React from 'react';
import { FieldType } from "../States/CostEditorStates";
import { EditItem } from "../States/CostBlockStates";
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField} from '@extjs/ext-react';
import { NamedId } from '../../Common/States/CommonStates';

Ext.require([
    'Ext.grid.plugin.CellEditing', 
]);

export interface ValueColumnProps {
    title: string
    type: FieldType,
    selectedItems: NamedId<number>[]
}

export interface EditProps {
    nameColumnTitle: string
    valueColumn: ValueColumnProps
    items: EditItem[]
    isEnableSave: boolean
    isEnableClear: boolean
    isEnableApplyFilters: boolean
    flex?: number
}

export interface EditActions {
    onItemEdited?(item: EditItem)
    onApplyFilters?()
    onCleared?()
    onSaving?()
}
  
export class EditGrid extends React.Component<EditProps & EditActions>  {



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
            selectable={{
                rows: true,
                cells: true,
                columns: true,
                drag: true,
                extensible: 'y'
            }}
            >
            <Toolbar docked="top">
                <Button 
                    text="Apply filters" 
                    flex={1} 
                    disabled={!props.isEnableApplyFilters}
                    handler={props.onApplyFilters}
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
                    handler={() => this.showSaveDialog()}
                />
            </Toolbar>
            </Grid>
        );
    }

    private showSaveDialog() {
        const { onSaving } = this.props;
    
        Ext.Msg.confirm(
          'Saving changes', 
          'Do you want to save the changes?',
          (buttonId: string) => onSaving && onSaving()
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
}