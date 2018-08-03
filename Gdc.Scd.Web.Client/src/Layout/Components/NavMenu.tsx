import * as React from 'react';
import PropTypes from 'prop-types';
import { TreeList } from '@extjs/ext-react';

declare var Ext:any;

Ext.require('Ext.data.TreeStore');

interface NavMenuProps {
    onItemClick: Function,
    selection: string
}

/**
 * The main navigation menu
 */
const NavMenu: React.SFC<NavMenuProps & any> = ({ 
    onItemClick, 
    selection, 
    ...props 
}) => (
    <TreeList 
        {...props}
        ui="nav"
        expanderFirst={false}
        onItemClick={(tree, item) => onItemClick(item.node.getId())}
        selection={selection}
        store={{
            root: {
                children: [
                    { id: '/', text: 'Home', iconCls: 'x-fa fa-home', leaf: true },
                    { id: '/about', text: 'About', iconCls: 'x-fa fa-info', leaf: true },
                    { id: '/pivot', text: 'Pivot grid', iconCls: 'x-fa fa-info', leaf: true },
                    { id: '/input-cost-elements', text: 'Input Cost Elements', iconCls: 'x-fa fa-info', leaf: true },
                    {
                        id: '/admin', text: 'Admin', iconCls: 'x-fa fa-cogs', disabled: true, children: [
                            {
                                id: '/admin/country-management',
                                text: 'Country Management', iconCls: 'x-fa fa-globe', leaf: true
                            },
                            {
                                id: '/admin/role-code-management',
                                text: 'Role codes', iconCls: 'x-fa fa-users', leaf: true
                            },
                            {
                                id: '/admin/warranty-group-management',
                                text: 'Warranty groups', iconCls: 'x-fa fa-industry', leaf: true
                            }]
                    }
                ]
            }
        }}
    />        
)

export default NavMenu;