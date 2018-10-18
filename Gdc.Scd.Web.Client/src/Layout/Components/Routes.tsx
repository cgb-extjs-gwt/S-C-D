import * as React from "react";
import { Route, Switch } from 'react-router-dom';

export interface RouteItem {
    path: string
    component
    exact?: boolean
}

export interface RoutesProps {
    routes: RouteItem[]
}

export class Routes extends React.PureComponent<RoutesProps> {
    // shouldComponentUpdate(nextProps: RoutesProps) {
    //     return this.props.routes != nextProps.routes;
    // }

    render() {
        const { routes } = this.props;

        return (
            routes &&
            routes.length > 0 &&
            <Switch>
                {
                    routes.map(route => (
                        <Route key={route.path} {...route} />
                    ))
                }
            </Switch>
        )
    }
}