import * as React from 'react'
import { Container, TitleBar, Button, Sheet, Panel } from '@extjs/ext-react';
import { Switch, Route, Redirect, withRouter } from 'react-router-dom'
import NavMenu from './NavMenu';
import { medium, large } from '../../responsiveFormulas';
import Home from '../../Test/Home/Home';
import About from '../../Test/About/About';
import { ScdPivotGrid } from '../../Test/ScdPivotGrid';
import { CostElementsInput } from '../../InputCostElement/Components/CostElementsInput';

interface LayoutProps { //extends LayoutPropsMethods {
    history: any,
    location: any,
}

// export interface LayoutPropsMethods {
//     onMenuItemClick: (pathName: string, history: History) => void
// }

/**
 * The main application view and routes
 */
class Layout extends React.Component<LayoutProps> {
    navigate = (path) => {
        this.props.history.push(path);
    }

    render() {
        const { location, history } = this.props;

        const navMenuDefaults = {
            onItemClick: this.navigate,
            //onItemClick: (path: string) => this.props.onMenuItemClick(path, history),
            selection: location.pathname
        }

        return (
            <Container fullscreen layout="fit">
                <Panel scrollable docked="left" shadow zIndex={2}>
                    <TitleBar title="SCD 2.0" docked="top"/>
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

                <Panel title="Page title" layout="fit">
                    <Switch>
                        <Route path="/" component={Home} exact/>
                        <Route path="/about" component={About}/>
                        <Route path="/pivot" component={ScdPivotGrid}/>
                        <Route path="/input-cost-elements" component={CostElementsInput}/>
                    </Switch>
                </Panel>
            </Container>
        );
    }
}

export default withRouter(Layout);