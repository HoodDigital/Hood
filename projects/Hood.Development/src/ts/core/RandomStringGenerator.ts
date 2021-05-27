export declare interface RandomStringGeneratorOptions {
    specials?: string;
    alpha?: string;
    numeric?: string;
    numSpecial?: number;
}

export default class RandomStringGenerator {
    options: RandomStringGeneratorOptions = {
        specials: '!@#$&*',
        alpha: 'abcdefghijklmnopqrstuvwxyz',
        numeric: '0123456789',
        numSpecial: 2
    };

    constructor(options?: RandomStringGeneratorOptions) {
        this.options = { ...this.options, ...options };
    }

    generate(length: number): string {

        let password = '';

        let len = Math.ceil((length - this.options.numSpecial) / 2);

        for (let i = 0; i < len; i++) {
            password += this.options.alpha.charAt(Math.floor(Math.random() * this.options.alpha.length));
            password += this.options.numeric.charAt(Math.floor(Math.random() * this.options.numeric.length));
        }

        for (let i = 0; i < this.options.numSpecial; i++)
            password += this.options.specials.charAt(Math.floor(Math.random() * this.options.specials.length));

        password = password.split('').sort(function () { return 0.5 - Math.random() }).join('');

        return password;

    }
}