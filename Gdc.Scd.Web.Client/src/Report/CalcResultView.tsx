import { Container, TabPanel } from "@extjs/ext-react";
import * as React from "react";
import { HwCostView } from "./HwCostView";
import { SwCostView } from "./SwCostView";
import { SwProactiveCostView } from "./SwProactiveCostView";
import { UserCountryService } from "../Dict/Services/UserCountryService";

export class CalcResultView extends React.Component<any, any> {
    private tabPanel;

    public componentDidMount() {
        this.tabPanel = this.refs.tabPanel;
    }

    public render() {
        const srv = new UserCountryService();
        srv.isCountryUser().then(x => {
            if (x) {
                this.tabPanel && this.tabPanel.items.items[2].tab.setHidden(true);       
            }
        });

        return (
            <Container layout="vbox">

                <TabPanel flex="1" tabBar={{ layout: { pack: 'left' } }} ref='tabPanel'>

                    <Container title="Hardware<br>service costs" layout="fit">
                        <HwCostView approved={false} />
                    </Container>

                    <Container title="Hardware<br>service costs<br>(approved)" layout="fit">
                        <HwCostView approved={true} />
                    </Container>

                    <Container title="Software &amp; Solution<br>service costs" layout="fit">
                        <SwCostView approved={false} />
                    </Container>

                    <Container title="Software &amp; Solution<br>service costs<br>(approved)" layout="fit">
                        <SwCostView approved={true} />
                    </Container>

                    <Container title="Software &amp; Solution<br>proactive cost" layout="fit">
                        <SwProactiveCostView approved={false} />
                    </Container>

                    <Container title="Software &amp; Solution<br>proactive cost<br>(approved)" layout="fit">
                        <SwProactiveCostView approved={true} />
                    </Container>

                </TabPanel>

            </Container>
        );
    }
}