module.exports = {
    tokens: {
        orbWidgetName: 'HACK26MyMoneyRules',
        clientFolderName: 'WebClient',
        areaFolderName: 'Areas',
    },
    presets: ['vue2'],
    presetConfiguration: {
        vue2: {
            plugins: ['webpack'],
            pluginConfiguration: {
                webpack: {
                    entry: {
                        app: './Scripts/app.ts',
                    },
                    autoInject: [
                        {
                            chunks: ['app'],
                            htmlFilePath: './Views/HACK26MyMoneyRules/Index.cshtml',
                            htmlTemplateFilePath: './Views/HACK26MyMoneyRules/Index.template.cshtml',
                        },
                    ],
                },
            },
        },
    },
};
