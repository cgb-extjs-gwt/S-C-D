import { Container, TabPanel } from "@extjs/ext-react";
import * as React from "react";
import { UserCountryService } from "../Dict/Services/UserCountryService";
import { HddCostView } from "./HddCostView";
import { HwCostView } from "./HwCostView";
import { SwCostView } from "./SwCostView";
import { SwProactiveCostView } from "./SwProactiveCostView";

export class CalcResultView extends React.Component<any, any> {

    public state = {
        isAdmin: false
    };

    public componentDidMount() {
        new UserCountryService().isAdminUser().then(x => this.setState({ isAdmin: x }));
    }

    public render() {

        let hdd = null, sw = null;

        if (this.state.isAdmin) {

            hdd = <Container title="Hdd retention<br>service costs" layout="fit">
                <HddCostView approved={false} />
            </Container>;

            sw = <Container title="Software &amp; Solution<br>service costs" layout="fit" >
                <SwCostView approved={false} />
            </Container>;
        }

        return (
            <Container layout="vbox">

                <TabPanel flex="1" tabBar={{ layout: { pack: 'left' } }}>

                    <Container title="Hardware<br>service costs" layout="fit">
                        <HwCostView approved={false} />
                    </Container>

                    <Container title="Hardware<br>service costs<br>(approved)" layout="fit">
                        <HwCostView approved={true} />
                    </Container>

                    {hdd}

                    <Container title="Hdd retention<br>service costs<br>(approved)" layout="fit">
                        <HddCostView approved={true} />
                    </Container>

                    {sw}

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