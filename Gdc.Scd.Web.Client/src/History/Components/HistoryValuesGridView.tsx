import * as React from "react";
import { Grid, Column, DateColumn, Container } from "@extjs/ext-react";
import { DateFormats } from "../../Common/Helpers/DateHelpers";
import { shomMask, hideMask } from "../../Common/Helpers/MaskHelper";

const EDIT_DATE_COLUMN_NAME = 'editDate';
const EDIT_USER_NAME_COLUMN_NAME = 'editUserName';
const VALUE_COLUMN_NAME = 'value';

export interface HistoryValuesGridViewProps {
    dataLoadUrl?: string
}

export class HistoryValuesGridView extends React.Component<HistoryValuesGridViewProps> {
    private readonly store;
    private grid:any & Grid;

    constructor(props: HistoryValuesGridViewProps) {
        super(props);

        this.store = this.buildStore(props.dataLoadUrl);
        this.init();
    }

    public render() {
        return (
             <Grid ref={x => this.grid = x} masked={{ xtype: "loadmask" }}
                store={this.store} deferEmptyText={false} emptyText="No history is available..."
                columnLines={true} border={true} minHeight="400">
                <DateColumn dataIndex={EDIT_DATE_COLUMN_NAME} text="Date" format={DateFormats.dateTime} flex={1} groupable={false}/>
                <Column dataIndex={EDIT_USER_NAME_COLUMN_NAME} text="User" flex={1} groupable={false}/>
                <Column dataIndex={VALUE_COLUMN_NAME} text="Value" flex={1} groupable={false}/>
            </Grid>
        );
    }

    private buildStore(dataLoadUrl: string) {
        return Ext.create('Ext.data.virtual.Store', {
            fields: [ 
                { name: EDIT_DATE_COLUMN_NAME, type: 'date' },
                { name: EDIT_USER_NAME_COLUMN_NAME, type: 'string' },
                { name: VALUE_COLUMN_NAME, mapping: ({ value }) => value == null ? ' ' : value }
            ],
            autoLoad: true,
            remoteSort: true,
            pageSize : 100,
            proxy: {
                type: 'ajax',
                url: dataLoadUrl,
                reader: { 
                    type: 'json',
                    rootProperty: 'items',
                    totalProperty: 'total'
                },
                listeners: {
                    exception: (proxy, response, operation) => {
                        this.onEndUpdate(this.store, operation);
                        console.log(operation.getError());
                    }
                }
            }
        });
    }

    private init() {
        this.store.on('endupdate', this.onEndUpdate, this);
    }

    private onEndUpdate(s, opt) {
        this.grid.setMasked(false);
    }
}