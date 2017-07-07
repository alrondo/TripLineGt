
#ifdef _MSC_VER
#pragma warning( push )
#pragma warning( disable : 4275 4251 )
#endif

#include <log4cxx/logger.h>
#include <log4cxx/propertyconfigurator.h>
#include <log4cxx/xml/domconfigurator.h>
#include <log4cxx/helpers/exception.h>
#include "log4cxx/basicconfigurator.h"
#include "log4cxx/rollingfileappender.h"
#include "log4cxx/patternlayout.h"
#include "log4cxx/consoleappender.h"
#include <log4cxx/ndc.h>
#include <locale.h>
#include <string>

using namespace log4cxx;
using namespace log4cxx::helpers;

#define DECLARE_LOGGER_PTR(var_name)              log4cxx::LoggerPtr var_name
#define GET_LOGGER_BY_NAME(var_name, logger_name) var_name = Logger::getLogger(logger_name)
#define GET_ROOT_LOGGER(var_name)                 var_name = Logger::getRootLogger()

#ifdef _MSC_VER
#pragma warning( pop )
#endif

namespace Av
{
  // Utilitary function to create a default logging scheme.
  // The "Default Logging" is this:
  // No Console Appender
  // Rolling File Appender. 6 files Max, each file is 10MB max.
  void configDefaultLogging(const std::string& logFileBaseName, log4cxx::LoggerPtr rootLogger)
  {
    static const LogString lConvPattern(LOG4CXX_STR("%d{yyyy-MM-dd HH:mm:ss,SSS} %p [%t] %c (%F:%L) %m%n"));
    LogString logFilrName(logFileBaseName);
    logFilrName += ".log";
    log4cxx::PatternLayoutPtr patternLayout = new PatternLayout();
    patternLayout->setConversionPattern(lConvPattern);
    log4cxx::RollingFileAppenderPtr rollingAppender = new log4cxx::RollingFileAppender();
    rollingAppender->setMaximumFileSize(10 * 1024 * 1024);
    rollingAppender->setMaxBackupIndex(5);
    rollingAppender->setOption(LOG4CXX_STR("append"), LOG4CXX_STR("true"));
    rollingAppender->setLayout(patternLayout);
    rollingAppender->setOption(LOG4CXX_STR("file"), logFilrName);
    log4cxx::helpers::Pool pool;
    rollingAppender->activateOptions(pool);

    BasicConfigurator::configure(rollingAppender);
    rootLogger->log(Level::getInfo(), logFilrName);
  }

  void addConsoleAppender(log4cxx::LoggerPtr logger)
  {
    static const LogString lConvPattern(LOG4CXX_STR("%m%n"));
    log4cxx::PatternLayoutPtr patternLayout = new PatternLayout();
    patternLayout->setConversionPattern(lConvPattern);
    log4cxx::ConsoleAppenderPtr consoleApp = new log4cxx::ConsoleAppender(patternLayout);
    logger->addAppender(consoleApp);
  }

}
