import { connect } from "react-redux";
import { withRouter } from 'react-router-dom';
import AvailabilityFeeGrid from "../../Admin/AvailabilityFee/AvailabilityFeeGrid";
import { CountryGrid } from "../../Admin/Country/CountryGrid";
import RoleCodesGrid from "../../Admin/RoleCode/RoleCodesGrid";
import UserRoleContainer from "../../Admin/UserRole/Containers/UserRoleContainer";
import { WarrantyGroupGrid } from "../../Admin/WarrantyGroup/WarrantyGroupGrid";
import * as Permissions from "../../Common/Constants/Permissions";
import { buildComponentUrl } from "../../Common/Services/Ajax";
import { MenuItem } from "../../Common/States/ExtStates";
import { ApprovalLayoutContainer } from "../../CostApproval/Components/ApprovalLayoutContainer";
import { CostEditorContainer } from "../../CostEditor/Components/CostEditorContainer";
import { CostImportContainer } from "../../CostImport/Components/CostImportContainer";
import { OwnApproveLayoutContainer } from "../../CostOwnApproval/Components/OwnApproveLayoutContainer";
import { PortfolioEditView, PortfolioHistoryView, PortfolioView } from "../../Portfolio/index";
import { CalcResultViewContainer, ReportView } from "../../Report/index";
import { TableViewContainer } from "../../TableView/Components/TableViewContainer";
import { loadMetaDataFromServer, openPage } from "../Actions/AppActions";
import { CommonState, Role } from "../States/AppStates";
import { Layout, LayoutActions, LayoutProps, RouteItem } from "./Layout";
import { ReportListViewContainer } from "../../Report/ReportListViewContainer";
import { PortfolioPivotGrid } from "../../PortfolioPivotGrid/Components/PortfolioPivotGrid";
import { ImportFromExcelContainer } from "../../TableView/Components/ImportFromExcelContainer";
import { ProjectListContainer } from "../../ProjectCalculator/Components/ProjectListContainer";
import { ProjectEditorContainder } from "../../ProjectCalculator/Components/ProjectEditorContainder";

export const Paths = {
    tableView: '/table-view',
    tableViewImport: '/table-view-import',
    projectCalculatorList: '/project-calculator/list',
    projectCalculatorEdit: '/project-calculator/edit'
}

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
    { path: Paths.tableView, text: 'Central data input', iconCls: 'x-fa fa-table', component: TableViewContainer, isMenuItem: true, permission: Permissions.TABLE_VIEW },
    { path: Paths.tableViewImport, text: 'Central data input import', component: ImportFromExcelContainer, permission: Permissions.TABLE_VIEW },
    { path: '/cost-import', text: 'Cost import', iconCls: 'x-fa fa-arrow-circle-o-up', component: CostImportContainer, isMenuItem: true, permission: Permissions.COST_IMPORT },
    { path: '/cost-approval', text: 'Approve cost elements', iconCls: 'x-fa fa-check-square-o', component: ApprovalLayoutContainer, isMenuItem: true, permission: Permissions.APPROVAL },
    { path: '/own-cost-approval', text: 'Own approve cost elements', iconCls: 'x-fa fa-check-square-o', component: OwnApproveLayoutContainer, isMenuItem: true, permission: Permissions.OWN_APPROVAL },
    { path: '/portfolio', text: 'Portfolio', iconCls: 'x-fa fa-suitcase', component: PortfolioView, isMenuItem: true, permission: Permissions.PORTFOLIO, exact: true },
    { path: '/portfolio/edit', component: PortfolioEditView, permission: Permissions.PORTFOLIO },
    { path: '/portfolio/history', component: PortfolioHistoryView, permission: Permissions.PORTFOLIO },
    { path: '/report', text: 'Calculation Result', iconCls: 'x-fa fa-calculator', component: CalcResultViewContainer, isMenuItem: true, permission: Permissions.REPORT, exact: true },
    { path: '/report/all', text: 'Reports', iconCls: 'x-fa fa-bar-chart', component: ReportListViewContainer, isMenuItem: true, permission: Permissions.REPORT, exact: true },
    { path: '/report/:name', component: ReportView, exact: true, permission: Permissions.REPORT },
    { path: Paths.projectCalculatorList, text: 'Project Calculator', iconCls: 'x-fa fa-laptop', component: ProjectListContainer, isMenuItem: true, permission: Permissions.PROJECT_CALCULATOR },
    { path: `${Paths.projectCalculatorEdit}/:id`, text: 'Edit Project', component: ProjectEditorContainder, permission: Permissions.PROJECT_CALCULATOR, exact: true },
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
            { path: '/admin/availability-fee', text: 'Availability Fee', iconCls: 'x-fa fa-cog', component: AvailabilityFeeGrid, isMenuItem: true, permission: Permissions.ADMIN },
            { path: '/admin/warranty-group-management', text: 'Warranty groups', iconCls: 'x-fa fa-industry', component: WarrantyGroupGrid, isMenuItem: true, permission: Permissions.ADMIN },                       
            { path: '/admin/user-role', text: 'User roles', iconCls: 'x-fa fa-users', component: UserRoleContainer, isMenuItem: true, permission: Permissions.ADMIN },
            { path: '/admin/role-code-management', text: 'Role codes', iconCls: 'x-fa fa-users', component: RoleCodesGrid, isMenuItem: false, permission: Permissions.ADMIN }
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

        let appVersion = app.appVersion;
        let isAuthorized = app.userRoles && app.userRoles.length > 0;

        return <LayoutProps>{
            title,
            menuItems,
            routes,
            appVersion,
            isAuthorized
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