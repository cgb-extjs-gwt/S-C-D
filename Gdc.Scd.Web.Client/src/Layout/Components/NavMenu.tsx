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
                    { id: '/scd/', text: 'Home', iconCls: 'x-fa fa-home', leaf: true },
                    { id: '/scd/about', text: 'About', iconCls: 'x-fa fa-info', leaf: true },
                    { id: '/scd/pivot', text: 'Pivot grid', iconCls: 'x-fa fa-info', leaf: true },
                    { id: '/scd/input-cost-elements', text: 'Input Cost Elements', iconCls: 'x-fa fa-info', leaf: true },
                    { id: '/scd/cost-approval', text: 'Approve cost elements', iconCls: 'x-fa fa-check-square-o', leaf: true},
                    { id: '/scd/capability-matrix', text: 'Portfolio', iconCls: 'x-fa fa-suitcase', leaf: true },
                    {
                        id: '/scd/admin', text: 'Admin', iconCls: 'x-fa fa-info', disabled: true, children: [{
                            id: '/scd/admin/country-management',
                            text: 'Country Management', iconCls: 'x-fa fa-globe', leaf: true
                        }, {
                                id: '/scd/admin/availability-fee',
                            text: 'Availability Fee', iconCls: 'x-fa fa-cog', leaf: true
                            },
                            {
                                id: '/scd/admin/warranty-group-management',
                                text: 'Warranty groups', iconCls: 'x-fa fa-industry', leaf: true
                            },                       
                             {
                                 id: '/scd/admin/user-role',
                                text: 'User roles', iconCls: 'x-fa fa-users', leaf: true
                            }
                        ]
                    }
                ]
            }
        }}
    />        
)

export default NavMenu;