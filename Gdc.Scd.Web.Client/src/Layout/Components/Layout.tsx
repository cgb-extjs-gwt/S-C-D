import { Container, Panel, TitleBar } from '@extjs/ext-react';
import * as React from 'react';
import { connect } from 'react-redux';
import { Route, Switch, withRouter } from 'react-router-dom';
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
import { CalcResultView } from '../../Report';
import { large, medium } from '../../responsiveFormulas';
import About from '../../Test/About/About';
import Home from '../../Test/Home/Home';
import { ScdPivotGrid } from '../../Test/ScdPivotGrid';
import { loadMetaDataFromServer } from '../Actions/AppActions';
import { CommonState } from '../States/AppStates';
import NavMenu from './NavMenu';

export const ROOT_LAYOUT_ID = "root-layout";

interface LayoutProps {
    title: string
    history: any,
    location: any,
    onInit?()
}

/**
 * The main application view and routes
 */
export class Layout extends React.Component<LayoutProps> {

    componentDidMount(){
        this.props.onInit();
    }

    navigate = (path) => {
        this.props.history.push(path);
    }

    render() {
        const { location, history, title } = this.props;

        const navMenuDefaults = {
            onItemClick: this.navigate,
            //onItemClick: (path: string) => this.props.onMenuItemClick(path, history),
            selection: location.pathname
        }

        return (
            <Container id={ROOT_LAYOUT_ID} fullscreen layout="fit">
                <Panel scrollable docked="left" shadow zIndex={2}>
                    <TitleBar title="SCD 2.0" docked="top" />
                    <NavMenu
                        {...navMenuDefaults}
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
                    />
                </Panel>

                <Panel title={title} layout="fit">
                    <Switch>
                        <Route path={buildComponentUrl("/")} component={CostEditorContainer} exact/>
                        <Route path={buildComponentUrl("/pivot")} component={ScdPivotGrid}/>
                        <Route path={buildComponentUrl("/input-cost-elements")} component={CostEditorContainer}/>
                        <Route path={buildComponentUrl("/admin/country-management")} component={ CountryGrid }/>
                        <Route path={buildComponentUrl("/cost-approval")} component={ ApprovalCostElementsLayout} />
                        <Route path={buildComponentUrl("/own-cost-approval")} component={ OwnApprovalCostElementsLayout} />
                        <Route path={buildComponentUrl("/report")} component={CalcResultView} />
                        <Route path={buildComponentUrl("/capability-matrix")} exact component={CapabilityMatrixView} />
                        <Route path={buildComponentUrl("/capability-matrix/edit")} component={CapabilityMatrixEditView} />
                        <Route path={buildComponentUrl("/admin/availability-fee")} component={AvailabilityFeeAdminGrid} />
                        <Route path={buildComponentUrl("/admin/role-code-management")} component={RoleCodesGrid} />
                        <Route path={buildComponentUrl("/admin/warranty-group-management")} component={WarrantyGroupGrid} />
                        <Route path={buildComponentUrl("/admin/user-role")} component={UserRoleContainer} />
                        <Route path={buildComponentUrl("/test")} component={About} />                   
                    </Switch>
                </Panel>
            </Container>
        );
    }
}

const containerFactory = connect<LayoutProps, {}, {}, CommonState>(
    state => ({
        title: state.app.currentPage && state.app.currentPage.title
    } as LayoutProps),
    dispatch => ({
        onInit: () => dispatch(loadMetaDataFromServer())
    })
);

export const LayoutContainer = withRouter(containerFactory(Layout));