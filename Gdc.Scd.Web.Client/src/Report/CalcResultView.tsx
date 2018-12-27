import { Container, TabPanel } from "@extjs/ext-react";
import * as React from "react";
import { HwCostView } from "./HwCostView";
import { SwCostView } from "./SwCostView";
import { SwProactiveCostView } from "./SwProactiveCostView";
import { AppService } from "../Layout/Services/AppService";

export class CalcResultView extends React.Component<any, any> {

    public state = {
        showAll: false
    }

    public constructor(props: any) {
        super(props);
        this.init();
    }

    public render() {

        let hwcosts = null;
        let swcosts = null;
        let proactive = null;

        if (this.state.showAll) {

            hwcosts = <Container title="Hardware<br>service costs" layout="fit">
                <HwCostView approved={false} />
            </Container>;

            swcosts = <Container title="Software &amp; Solution<br>service costs" layout="fit">
                <SwCostView approved={false} />
            </Container>;

            proactive = <Container title="Software &amp; Solution<br>proactive cost" layout="fit">
                <SwProactiveCostView approved={false} />
            </Container>;
        }

        return (
            <Container layout="vbox">

                <TabPanel flex="1" tabBar={{ layout: { pack: 'left' } }}>

                    {hwcosts}

                    <Container title="Hardware<br>service costs<br>(approved)" layout="fit">
                        <HwCostView approved={true} />
                    </Container>

                    {swcosts}

                    <Container title="Software &amp; Solution<br>service costs<br>(approved)" layout="fit">
                        <SwCostView approved={true} />
                    </Container>

                    {proactive}

                    <Container title="Software &amp; Solution<br>proactive cost<br>(approved)" layout="fit">
                        <SwProactiveCostView approved={true} />
                    </Container>

                </TabPanel>

            </Container>
        );
    }

    private init() {
        new AppService().hasGlobalRole().then(x => this.setState({ showAll: x }));
    }
}