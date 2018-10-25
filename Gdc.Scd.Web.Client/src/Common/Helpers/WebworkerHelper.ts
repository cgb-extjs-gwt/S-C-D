export class WebworkerHelper {

    public static run(fn) {
        return new Worker(URL.createObjectURL(new Blob(['(' + fn + ')()'])));
    }

}