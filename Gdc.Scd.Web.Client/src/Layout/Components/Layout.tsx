import { Container, Panel, TitleBar } from '@extjs/ext-react';
import * as React from 'react';
import { Route, Switch } from 'react-router-dom';
import { MenuItem } from '../../Common/States/ExtStates';
import { NavMenuContainer } from './NavMenuContainer';
import { AlertPanel } from './AlertPanel';
import Unauth from './Unauth';
import Default from './Default';

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
    appVersion: string
    isAuthorized: boolean
}

/**
 * The main application view and routes
 */
export class Layout extends React.Component<LayoutProps> {
    componentDidMount(){
        this.props.onInit();
    }

    render() {
        const { title, routes, menuItems, appVersion, isAuthorized } = this.props;
        const titleBar = appVersion ? `SCD 2.0 ver.${appVersion}` : "SCD 2.0";

        return (
            <Container id={ROOT_LAYOUT_ID} fullscreen layout="fit">
                <Panel scrollable docked="left" shadow zIndex={2}>
                    <TitleBar title={titleBar} docked="top" /> 
                    <NavMenuContainer items={menuItems} />
                </Panel>

                <AlertPanel />

                <Panel title={title} layout="fit">
                    <Switch>
                        {
                            routes.map(route => (
                                <Route key={route.path} {...route} />
                            ))
                        }
                        {
                            isAuthorized ? <Route component={Default} /> : 
                                <Route component={Unauth} />
                        }
                    </Switch>
                </Panel>
            </Container>
        );
    }
}
