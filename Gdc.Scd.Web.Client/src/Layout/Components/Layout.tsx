import { Container, Panel, TitleBar } from '@extjs/ext-react';
import * as React from 'react';
import { connect } from 'react-redux';
import { Route, Switch } from 'react-router-dom';
import AvailabilityFeeAdminGrid from '../../Admin/AvailabilityFee/AvailabilityFeeAdminGrid';
import CountryGrid from '../../Admin/Country/containers/CountryGrid';
import RoleCodesGrid from '../../Admin/RoleCode/RoleCodesGrid';
import UserRoleContainer from '../../Admin/UserRole/Containers/UserRoleContainer';
import WarrantyGroupGrid from '../../Admin/WarrantyGroup/WarrantyGroupGrid';
import { CapabilityMatrixEditView, CapabilityMatrixView } from '../../CapabilityMatrix';
import { buildComponentUrl } from "../../Common/Services/Ajax";
import ApprovalCostElementsLayout from '../../CostApproval/Components/ApprovalCostElementsLayout';
import { OwnApprovalCostElementsLayout } from '../../CostApproval/Components/OwnApprovalCostElementsLayout';
import { CostEditorContainer } from '../../CostEditor/Components/CostEditorContainer';
import { CalcResultView, ReportListView, ReportView } from '../../Report';
import { large, medium } from '../../responsiveFormulas';
import { ScdPivotGrid } from '../../Test/ScdPivotGrid';
import { loadMetaDataFromServer, openPage } from '../Actions/AppActions';
import { CommonState } from '../States/AppStates';
import NavMenu from './NavMenu';
import { TableViewContainer } from '../../TableView/Components/TableViewContainer';
import { MenuItem } from '../../Common/States/ExtStates';
import { NavMenuContainer } from './NavMenuContainer';
import { Routes } from './Routes';

Ext.require(['Ext.data.ChainedStore'])

export const ROOT_LAYOUT_ID = "root-layout";

export interface LayoutActions {
    onInit?()
}

export interface RouteItem {
    path: string
    component
    exact?: boolean
}

export interface LayoutProps extends LayoutActions {
    title: string
    //history: any
    //location: any
    routes: RouteItem[]
    menuItems: MenuItem[]
}

/**
 * The main application view and routes
 */
export class Layout extends React.Component<LayoutProps> {
    componentDidMount(){
        this.props.onInit();
    }

    render() {
        const { title, routes, menuItems } = this.props;

        return (
            <Container id={ROOT_LAYOUT_ID} fullscreen layout="fit">
                <Panel scrollable docked="left" shadow zIndex={2}>
                    <TitleBar title="SCD 2.0" docked="top" />
                    <NavMenuContainer items={menuItems} />
                </Panel>

                <Panel title={title} layout="fit">
                    <Routes routes={routes}/>
                    {/* <Switch>
                        <Route path={buildComponentUrl("/")} component={CostEditorContainer} exact/>
                        <Route path={buildComponentUrl("/pivot")} component={ScdPivotGrid}/>
                        <Route path={buildComponentUrl("/input-cost-elements")} component={CostEditorContainer}/>
                        <Route path={buildComponentUrl("/table-view")} component={TableViewContainer}/>
                        <Route path={buildComponentUrl("/admin/country-management")} component={ CountryGrid }/>
                        <Route path={buildComponentUrl("/cost-approval")} component={ ApprovalCostElementsLayout} />
                        <Route path={buildComponentUrl("/own-cost-approval")} component={ OwnApprovalCostElementsLayout} />
                        <Route path={buildComponentUrl("/report")} exact component={CalcResultView} />
                        <Route path={buildComponentUrl("/report/all")} exact component={ReportListView} />
                        <Route path={buildComponentUrl("/report/:name")} exact component={ReportView} />
                        <Route path={buildComponentUrl("/capability-matrix")} exact component={CapabilityMatrixView} />
                        <Route path={buildComponentUrl("/capability-matrix/edit")} component={CapabilityMatrixEditView} />
                        <Route path={buildComponentUrl("/admin/availability-fee")} component={AvailabilityFeeAdminGrid} />
                        <Route path={buildComponentUrl("/admin/role-code-management")} component={RoleCodesGrid} />
                        <Route path={buildComponentUrl("/admin/warranty-group-management")} component={WarrantyGroupGrid} />
                        <Route path={buildComponentUrl("/admin/user-role")} component={UserRoleContainer} />
                    </Switch> */}
                </Panel>
            </Container>
        );
    }
}
