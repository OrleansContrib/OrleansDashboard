build:
	@echo Lint
	@eslint . --ext .js --ext .jsx
	@echo Test
	@mocha
	@echo Build
	@browserify  -t babelify index.jsx  -g [ envify --NODE_ENV production ] -g uglifyify  | uglifyjs --compress warnings=false --mangle > ../OrleansDashboard/index.min.js
	@wc -c ../OrleansDashboard/index.min.js
	@echo Done

debug:
	@browserify -t babelify index.jsx --outfile ../OrleansDashboard/index.min.js
	@echo Done
