import * as React from 'react';
import { Grid, CheckColumn, Column, Toolbar, Button } from '@extjs/ext-react';
import { CheckItem } from '../States/CostBlockStates';
import { NamedId } from '../../Common/States/CommonStates';

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
        data: items && items.slice(),
        listeners: {
          update: onSelectionChanged && 
                  ((store, record, operation, modifiedFieldNames, details) => 
                    onSelectionChanged(record.data, record.data.isChecked))
        }
    });

    return (
        <Grid 
            title={title || ''} 
            store={store} 
            flex={flex} 
            shadow
            cls="filter-grid"
            height={height}
            columnLines={true}
        >
            <CheckColumn width="70" dataIndex="isChecked"/>
            <Column text={valueColumnText || ''}  dataIndex="name" flex={1}/>

            <Toolbar docked="bottom">
                <Button 
                    text="Reset" 
                    flex={1} 
                    handler={() => onReset && onReset()}
                    disabled={!items || items.findIndex(item => item.isChecked) === -1}
                />
            </Toolbar>
        </Grid>
    );
}
