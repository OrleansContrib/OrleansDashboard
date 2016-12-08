build:
	@echo Lint
	@eslint . --ext .js --ext .jsx
	@echo Test
	@mocha
	@echo Build
	@browserify -t babelify index.jsx --outfile ../OrleansDashboard/index.js
	@echo Compress
	@uglifyjs ../OrleansDashboard/index.js  --output ../OrleansDashboard/index.min.js --compress warnings=false
	@wc -c ../OrleansDashboard/index.js
	@wc -c ../OrleansDashboard/index.min.js
	@echo Done

debug:
	@browserify -t babelify index.jsx --outfile ../OrleansDashboard/index.min.js
	@echo Done
