import { TreeList } from '@extjs/ext-react';
import * as React from 'react';
import { buildComponentUrl } from "../../Common/Services/Ajax";

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
                    { id: buildComponentUrl('/'), text: 'Home', iconCls: 'x-fa fa-home', leaf: true },
                    { id: buildComponentUrl('/about'), text: 'About', iconCls: 'x-fa fa-info', leaf: true },
                    { id: buildComponentUrl('/pivot'), text: 'Pivot grid', iconCls: 'x-fa fa-info', leaf: true },
                    { id: buildComponentUrl('/input-cost-elements'), text: 'Input Cost Elements', iconCls: 'x-fa fa-info', leaf: true },
                    { id: buildComponentUrl('/cost-approval'), text: 'Approve cost elements', iconCls: 'x-fa fa-check-square-o', leaf: true},
                    { id: buildComponentUrl('/report'), text: 'Review process', iconCls: 'x-fa fa-balance-scale', leaf: true },
                    { id: buildComponentUrl('/capability-matrix'), text: 'Portfolio', iconCls: 'x-fa fa-suitcase', leaf: true },
                    {
                        id: buildComponentUrl('/admin'), text: 'Admin', iconCls: 'x-fa fa-info', disabled: true, children: [{
                            id: buildComponentUrl('/admin/country-management'),
                            text: 'Country Management', iconCls: 'x-fa fa-globe', leaf: true
                        }, {
                                id: buildComponentUrl('/admin/availability-fee'),
                            text: 'Availability Fee', iconCls: 'x-fa fa-cog', leaf: true
                            },
                            {
                                id: buildComponentUrl('/admin/warranty-group-management'),
                                text: 'Warranty groups', iconCls: 'x-fa fa-industry', leaf: true
                            },                       
                             {
                                 id: buildComponentUrl('/admin/user-role'),
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