import { TreeList } from '@extjs/ext-react';
import * as React from 'react';
import { MenuItem } from '../../Common/States/ExtStates';
import { large, medium } from '../../responsiveFormulas';

declare var Ext:any;

Ext.require('Ext.data.TreeStore');

export interface NavMenuActions {
    onItemClick?(item: MenuItem)
}

export interface NavMenuProps extends NavMenuActions {
    selection?: string
    items: MenuItem[]  
}

/**
 * The main navigation menu
 */
const NavMenu: React.SFC<NavMenuProps> = ({ 
    onItemClick, 
    selection, 
    items
}) => (
    <TreeList 
        ui="nav"
        expanderFirst={false}
        onItemClick={(tree, item) => onItemClick(item.node.data)}
        selection={selection}
        responsiveConfig={{
            [medium]: {
                micro: true,
                width: 56
            },
            [large]: {
                micro: false,
                width: 200
            }
        }}
        store={{
            root: {
                children: items
            }
        }}
    />        
)

export default NavMenu;