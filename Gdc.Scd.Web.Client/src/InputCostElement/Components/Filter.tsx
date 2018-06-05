import * as React from 'react';
import { Grid, CheckColumn, Column, Toolbar, Button } from '@extjs/ext-react';
import { NamedId } from '../../Common/States/NamedId';
import { CheckItem } from '../States/CostBlock';

export interface FilterActions {
    onSelectionChanged?: (item: NamedId, isSelected: boolean) => void
    onReset?: () => void
}

export interface FilterProps extends FilterActions {
    title?: string
    valueColumnText?: string
    items: CheckItem[]
    flex?: number
    height?: string
}

export const Filter: React.StatelessComponent<FilterProps> = ({ 
    title, 
    items, 
    flex, 
    valueColumnText,
    height,
    onSelectionChanged,
    onReset
}) => {
    
    const store = Ext.create('Ext.data.Store', {
        //groupField: 'isChecked',
        data: items
    });

    return (
        <Grid 
            title={title || ''} 
            store={store} 
            flex={flex} 
            shadow
            cls="filter-grid"
            minHeight="300"
            height={height}
            columnLines={true}
        >
            <CheckColumn 
                dataIndex="isChecked"
                onCheckChange={(cell, rowIndex, isChecked: boolean, record) =>{
                    onSelectionChanged && onSelectionChanged(record.data, isChecked)
                }}
            />
            <Column text={valueColumnText || ''}  dataIndex="name" flex={1}/>

            <Toolbar docked="bottom">
                <Button text="Reset" flex={1} handler={() => onReset && onReset()}/>
            </Toolbar>
        </Grid>
    );
}
