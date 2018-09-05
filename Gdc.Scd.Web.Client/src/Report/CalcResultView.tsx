import { Container, TabPanel } from "@extjs/ext-react";
import * as React from "react";
import { HardwareCostView } from "./HardwareCostView";
import { SoftwareCostView } from "./SoftwareCostView";

export class CalcResultView extends React.Component<any, any> {

    public render() {

        return (
            <Container layout="vbox">

                <TabPanel flex="1" tabBar={{ layout: { pack: 'left' } }}>

                    <Container title="Hardware service costs" layout="fit">
                        <HardwareCostView />
                    </Container>

                    <Container title="Software &amp; Solution service costs" layout="fit">
                        <SoftwareCostView />
                    </Container>

                </TabPanel>

            </Container>
        );
    }
}