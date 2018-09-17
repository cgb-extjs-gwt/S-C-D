import { NamedId } from "../../Common/States/CommonStates";
import { IDictService } from "../Services/IDictService";
import { fakeAvailability } from "./FakeAvailability";
import { fakeCountries } from "./FakeCountries";
import { fakeDuration } from "./FakeDuration";
import { fakeReactTimeTypes } from "./FakeReactTimeTypes";
import { fakeReactTypes } from "./FakeReactTypes";
import { fakeServiceLocationTypes } from "./FakeServiceLocationTypes";
import { fakeSog } from "./FakeSog";
import { fakeWG } from "./FakeWG";
import { fakeYears } from "./FakeYear";

export class FakeDictService implements IDictService {

    public getCountries(): Promise<NamedId[]> {
        return this.fromResult(fakeCountries);
    }

    public getWG(): Promise<NamedId[]> {
        return this.fromResult(fakeWG);
    }

    public getSog(): Promise<NamedId<string>[]> {
        return this.fromResult(fakeSog);
    }

    public getAvailabilityTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeAvailability);
    }

    public getDurationTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeDuration);
    }

    public getYears(): Promise<NamedId<string>[]> {
        return this.fromResult(fakeYears);
    }

    public getReactTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeReactTypes);
    }

    public getReactionTimeTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeReactTimeTypes);
    }

    public getServiceLocationTypes(): Promise<NamedId[]> {
        return this.fromResult(fakeServiceLocationTypes);
    }

    private fromResult<T>(value: T): Promise<T> {
        return Promise.resolve(value);
    }
}
