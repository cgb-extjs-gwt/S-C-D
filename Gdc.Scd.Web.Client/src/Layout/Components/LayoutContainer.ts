import { connect } from "react-redux";
import { withRouter } from 'react-router-dom';
import { LayoutProps, LayoutActions, ROOT_LAYOUT_ID, Layout, RouteItem } from "./Layout";
import { CommonState, Role } from "../States/AppStates";
import { loadMetaDataFromServer, openPage } from "../Actions/AppActions";
import { MenuItem } from "../../Common/States/ExtStates";
import { buildComponentUrl } from "../../Common/Services/Ajax";
import { CostEditorContainer } from "../../CostEditor/Components/CostEditorContainer";
import { TableViewContainer } from "../../TableView/Components/TableViewContainer";
import ApprovalCostElementsLayout from "../../CostApproval/Components/ApprovalCostElementsLayout";
import { OwnApprovalCostElementsLayout } from "../../CostApproval/Components/OwnApprovalCostElementsLayout";
import { CapabilityMatrixView } from "../../CapabilityMatrix/CapabilityMatrixView";
import { CalcResultView } from "../../Report/CalcResultView";
import { ReportListView } from "../../Report/ReportListView";
import CountryGrid from "../../Admin/Country/containers/CountryGrid";
import AvailabilityFeeAdminGrid from "../../Admin/AvailabilityFee/AvailabilityFeeAdminGrid";
import WarrantyGroupGrid from "../../Admin/WarrantyGroup/WarrantyGroupGrid";
import UserRoleContainer from "../../Admin/UserRole/Containers/UserRoleContainer";
import { ReportView } from "../../Report/index";
import * as Permissions from "../../Common/Constants/Permissions"
import { CapabilityMatrixEditView } from "../../CapabilityMatrix/index";
import { Dispatch } from "redux";

interface RouteMenuItem extends RouteItem {
    text?: string
    iconCls?: string
    disabled?: boolean
    isMenuItem?: boolean
    permission?: string
    children?: RouteMenuItem[]
}

export interface LayoutContainerProps {
    location?
}

const buildRouteMenuItems = () => <RouteMenuItem[]>[
    { path: '/input-cost-elements', text: 'Input Cost Elements', iconCls: 'x-fa fa-info', component: CostEditorContainer, isMenuItem: true, permission: Permissions.COST_EDITOR },
    { path: '/table-view', text: 'Table View', iconCls: 'x-fa fa-info', component: TableViewContainer, isMenuItem: true, permission: Permissions.TABLE_VIEW },
    { path: '/cost-approval', text: 'Approve cost elements', iconCls: 'x-fa fa-check-square-o', component: ApprovalCostElementsLayout, isMenuItem: true, permission: Permissions.APPROVAL },
    { path: '/own-cost-approval', text: 'Own approve cost elements', iconCls: 'x-fa fa-check-square-o', component: OwnApprovalCostElementsLayout, isMenuItem: true, permission: Permissions.OWN_APPROVAL },
    { path: '/capability-matrix', text: 'Portfolio', iconCls: 'x-fa fa-suitcase', component: CapabilityMatrixView, isMenuItem: true, permission: Permissions.PORTFOLIO, exact: true },
    { path: '/capability-matrix/edit', component: CapabilityMatrixEditView, permission: Permissions.PORTFOLIO },
    { path: '/report', text: 'Review process', iconCls: 'x-fa fa-balance-scale', component: CalcResultView, isMenuItem: true, permission: Permissions.REPORT, exact: true },
    { path: '/report/all', text: 'Report', iconCls: 'x-fa fa-bar-chart', component: ReportListView, isMenuItem: true, permission: Permissions.REPORT, exact: true },
    { path: '/report/:name', component: ReportView, exact: true, permission: Permissions.REPORT },
    {
        path: '/admin', 
        text: 'Admin', 
        iconCls: 'x-fa fa-info', 
        disabled: true, 
        component: null,
        isMenuItem: true,
        permission: Permissions.ADMIN,
        children: [
            { path: '/admin/country-management', text: 'Country Management', iconCls: 'x-fa fa-globe', component: CountryGrid, isMenuItem: true, permission: Permissions.ADMIN }, 
            { path: '/admin/availability-fee', text: 'Availability Fee', iconCls: 'x-fa fa-cog', component: AvailabilityFeeAdminGrid, isMenuItem: true, permission: Permissions.ADMIN },
            { path: '/admin/warranty-group-management', text: 'Warranty groups', iconCls: 'x-fa fa-industry', component: WarrantyGroupGrid, isMenuItem: true, permission: Permissions.ADMIN },                       
            { path: '/admin/user-role', text: 'User roles', iconCls: 'x-fa fa-users', component: UserRoleContainer, isMenuItem: true, permission: Permissions.ADMIN }
        ]
    }
]

const filterByPermissions = (userPermissions: Set<string>, routeMenuItems: RouteMenuItem[]) => {
    const filteredItems: RouteMenuItem[] = [];

    for (const routeMenuItem of routeMenuItems) {
        if (!routeMenuItem.permission || userPermissions.has(routeMenuItem.permission)) {
            filteredItems.push({
                ...routeMenuItem,
                children: routeMenuItem.children && filterByPermissions(userPermissions, routeMenuItem.children)
            })
        }
    }

    return filteredItems;
}

const getUserPermissions = (userRoles: Role[]) => {
    const permissions = new Set<string>();

    userRoles.forEach(
        role => role.permissions.forEach(
            permission => permissions.add(permission)
        )
    )

    return permissions;
}

const buildMenuItems = (routeMenuItems: RouteMenuItem[]) => 
    routeMenuItems.filter(routeMenuItem => routeMenuItem.isMenuItem).map(
        ({ path, text, iconCls, disabled, children }: RouteMenuItem) => { 
            let menuItemChildren: MenuItem[];
            let leaf: boolean;

            if (children) {
                leaf = false;
                menuItemChildren = buildMenuItems(children);
            }
            else {
                leaf = true;
            }

            return <MenuItem>{
                id: buildComponentUrl(path),
                text,
                iconCls,
                disabled,
                leaf,
                children: menuItemChildren
            }
        }
)

const buildRouteItems = (routeMenuItems: RouteMenuItem[]) => {
    const routes: RouteItem[] = [];

    for (const { path, component, exact, children } of routeMenuItems) {
        if (component) {
            routes.push({
                path: buildComponentUrl(path), 
                component,
                exact
            })
        }

        if (children) {
            routes.push(...buildRouteItems(children));
        }
    }

    return routes;
}

const findMenuItem = (menuItems: MenuItem[], id: string) => {
    let result: MenuItem = null;

    for (const menuItem of menuItems) {
        if (menuItem.id === id) {
            result = menuItem;

            break;
        }
        else if (menuItem.children) {
            result = findMenuItem(menuItem.children, id);

            if (result) {
                break;
            }
        }
    }

    return result;
}

const buildMapStateToProps = () => {
    let prevUserRoles: Role[];
    let menuItems: MenuItem[] = [];
    let routes: RouteItem[] = [];

    return ({ app }: CommonState) => {
        if (app.userRoles != prevUserRoles) {
            prevUserRoles = app.userRoles;

            const permissions = getUserPermissions(app.userRoles);
            const routeMenuItems = filterByPermissions(permissions, buildRouteMenuItems());

            menuItems = buildMenuItems(routeMenuItems);
            routes = buildRouteItems(routeMenuItems);
        }

        let title: string;

        if (app.currentPage && app.currentPage.id) {
            const currentMenuItem = findMenuItem(menuItems, app.currentPage.id);

            if (currentMenuItem) {
                title = currentMenuItem.text;
            }
        }

        return <LayoutProps>{
            title,
            menuItems,
            routes
        }
    }
}

const containerFactory = connect<LayoutProps, LayoutActions, LayoutContainerProps, CommonState>(
    buildMapStateToProps(),
    (dispatch, { location }) => (<LayoutActions>{ 
        onInit: () => {
            dispatch(loadMetaDataFromServer());
            dispatch(openPage(location.pathname));
        }
    }),
    null,
    {
        areStatesEqual: (nextState, prevState) => nextState.app == prevState.app
    }
);

export const LayoutContainer = withRouter(containerFactory(Layout));