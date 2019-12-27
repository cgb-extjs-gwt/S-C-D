import * as React from "react";
import { Grid, CheckColumn, Column, Toolbar, Button } from "@extjs/ext-react";
import { NamedId } from "../../Common/States/CommonStates";
import { Store } from "../../Common/States/ExtStates";

const CHECK_DATA_INDEX = 'checked';
const DATA_DATA_INDEX = 'data';
const NAME_DATA_INDEX = 'name';

export interface NotifyGridActions {
    onWindowNotifyButtonClick?(selectedItems: NamedId<number>[], store: Store)
}

export interface NotifyGridProps extends NotifyGridActions {
    dataLoadUrl: string
}

export class NotifyGrid extends React.PureComponent<NotifyGridProps> {
    private readonly store = this.buildStore();
    private grid;

    public render() {
        return (
            <Grid 
                ref={this.setGridRef}
                store={this.store} 
                masked={{ xtype: "loadmask" }} 
                deferEmptyText={false} 
                emptyText="No items is available..."
                columnLines={true} 
                border={true} 
                minHeight="400"
            >
                <CheckColumn dataIndex={CHECK_DATA_INDEX} groupable={false} sortable={false}/>
                <Column dataIndex={NAME_DATA_INDEX} flex={1} text="Name" groupable={false}/>
                <Toolbar docked="bottom" layout={{type: 'vbox', align: 'center'}}>
                    <Button text="Notify Country users" handler={this.onWindowNotifyClick} />
                </Toolbar>
            </Grid>
        );
    }

    private setGridRef = grid => {
        this.grid = grid;
    }

    private buildStore(): Store {
        return Ext.create('Ext.data.Store', {
            fields: [ 
                { name: CHECK_DATA_INDEX, type: 'bool' },
                { name: NAME_DATA_INDEX, type: 'string' },
            ],
            autoLoad: true,
            proxy: {
                type: 'ajax',
                url: this.props.dataLoadUrl,
                reader: { 
                    type: 'json',
                    transform: data => {
                        const items = data as NamedId<number>[];

                        return items.map(item => ({
                            [CHECK_DATA_INDEX]: false,
                            [NAME_DATA_INDEX]: item.name,
                            [DATA_DATA_INDEX]: item
                        }));
                    }
                },
                listeners: {
                    exception: (proxy, response, operation) => {
                        this.grid.setMasked(false);
                        console.log(operation.getError());
                    }
                }
            }
        });
    }

    private onWindowNotifyClick = () => {
        const { onWindowNotifyButtonClick } = this.props;

        if (onWindowNotifyButtonClick) {
            const selectedItems: NamedId<number>[] = [];

            this.store.each(record => {
                if (record.get(CHECK_DATA_INDEX)) {
                    const namedId = record.get(DATA_DATA_INDEX);

                    selectedItems.push(namedId);
                }
            })

            onWindowNotifyButtonClick(selectedItems, this.store);
        }
    }
}