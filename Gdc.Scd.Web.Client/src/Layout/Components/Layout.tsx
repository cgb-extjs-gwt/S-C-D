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
                    <Switch>
                        {
                            routes.map(route => (
                                <Route key={route.path} {...route} />
                            ))
                        }
                    </Switch>
                </Panel>
            </Container>
        );
    }
}
