import * as React from 'react';
import { FieldType } from "../CostEditor/States/CostEditorStates";
import { EditItem } from "../CostEditor/States/CostBlockStates";
import { ComboBoxField, Grid, Column, Toolbar, Button, SelectField } from '@extjs/ext-react';
import { NamedId } from '../Common/States/CommonStates';

Ext.require([
    'Ext.grid.plugin.Editable',
    'Ext.grid.plugin.CellEditing',
]);

export default class RoleCodesGrid extends React.Component {

    store = Ext.create('Ext.data.Store', {
        fields: ['id', 'name'],
        autoLoad: true,
        //autoSync: true,
        pageSize: 0,
        proxy: {
            type: 'ajax',
            //url: '/api/rolecode/getall'
            writer: {
                type: 'json',        
                writeAllFields: true,             
                idProperty: "id",
                allowSingle: false
            },
            reader: {
                type: 'json',        
                idProperty: "id"
            },
            api: {
                read: '/api/rolecode/GetAll',
                update: '/api/rolecode/SaveAll'
            }
        }
    });

    render() {
        const props = this.props;
        const store = this.store;
        return (
            <Grid
                title='Role codes'
                shadow
                store={store}
                platformConfig={{
                    desktop: {
                        plugins: {
                            gridcellediting: true
                        }
                    },
                    '!desktop': {
                        plugins: {
                            grideditable: true
                        }
                    }
                }}
            >
                <Column
                    text="ID"
                    width="150"
                    dataIndex="id"                   
                />
                <Column
                    text="Role code"
                    width="150"
                    dataIndex="name"
                    editable
                />       
                <Toolbar docked="bottom">
                    <Button
                        text="Cancel"
                        flex={1}
                        //disabled={!props.isEnableClear}
                        handler={() => this.store.rejectChanges()}
                    />
                    <Button
                        text="Save"
                        flex={1}
                        //disabled={!props.isEnableSave}
                        handler={() => console.log(store.sync({
                            callback: function () {
                                console.log('callback', arguments);
                            },
                            success: function () {
                                console.log('success', arguments);
                            },
                            failure: function () {
                                this.store.rejectChanges();
                                console.log('failure', arguments);
                            }
                        }))}
                    />
                </Toolbar>
            </Grid>
        )
    }

    //private showSaveDialog() {
    //    const { onSaving } = this.props;

    //    Ext.Msg.confirm(
    //        'Saving changes',
    //        'Do you want to save the changes?',
    //        (buttonId: string) => onSaving && onSaving()
    //    );
    //}

    //private showClearDialog() {
    //    const { onCleared } = this.props;

    //    Ext.Msg.confirm(
    //        'Clearing changes',
    //        'Do you want to clear the changes??',
    //        (buttonId: string) => onCleared && onCleared()
    //    );
    //}
}