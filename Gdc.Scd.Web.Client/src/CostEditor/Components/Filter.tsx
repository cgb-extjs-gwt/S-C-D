import * as React from 'react';
import { Grid, CheckColumn, Column, Toolbar, Button } from '@extjs/ext-react';
import { CheckItem } from '../States/CostBlockStates';
import { NamedId } from '../../Common/States/CommonStates';
import { Store } from '../../Common/States/ExtStates';
import { objectPropsEqual } from '../../Common/Helpers/CommonHelpers';

export interface FilterActions {
    onSelectionChanged?: (item: NamedId<number>, isSelected: boolean) => void
    onReset?: () => void
}

export interface FilterProps extends FilterActions {
    title?: string
    valueColumnText?: string
    items: CheckItem[]
    flex?: number
    height?: string
}

export class Filter extends React.Component<FilterProps> {
    private store: Store<CheckItem>

    constructor(props: FilterProps) {
        super(props);

        const { items } = props;

        this.store = this.createStore(items);
    }

    public shouldComponentUpdate(nextProps: FilterProps) {
        return !objectPropsEqual(this.props, nextProps, 'title', 'valueColumnText', 'items', 'flex', 'height');
    }

    public componentWillReceiveProps(nextProps: FilterProps) {
        if (this.props.items != nextProps.items) {
            this.store = this.createStore(nextProps.items);
        }
    }

    public render() {
        const { title, flex, valueColumnText, height, onReset, items } = this.props;
    
        return (
            <Grid 
                title={title || ''} 
                store={this.store} 
                flex={flex} 
                shadow
                cls="filter-grid"
                height={height}
                columnLines={true}
            >
                <CheckColumn width="70" dataIndex="isChecked" sortable={false}/>
                <Column text={valueColumnText || ''}  dataIndex="name" flex={1}/>
    
                <Toolbar docked="bottom">
                    <Button 
                        text="Reset" 
                        flex={1} 
                        handler={this.onReset}
                        disabled={!items || items.findIndex(item => item.isChecked) === -1}
                    />
                </Toolbar>
            </Grid>
        );
    }

    private onReset = () => {
        const { onReset } = this.props;

        onReset && onReset();
    }

    private onSelectionChanged = (store, record, operation, modifiedFieldNames, details) => {
        const { onSelectionChanged } = this.props;

        onSelectionChanged && onSelectionChanged(record.data, record.data.isChecked);
    }

    private createStore(items: CheckItem[]) {
        return Ext.create('Ext.data.Store', {
            data: items || [],
            listeners: {
              update: this.onSelectionChanged
            }
        });
    }
}