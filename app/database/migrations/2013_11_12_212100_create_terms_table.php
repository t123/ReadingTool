<?php

use Illuminate\Database\Schema\Blueprint;
use Illuminate\Database\Migrations\Migration;

class CreateTermsTable extends Migration {

    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up() {
        Schema::create('terms', function(Blueprint $table) {
            $table->increments('id');
            $table->enum('state', array('known', 'unknown', 'notseen', 'ignored'));
            $table->string('phrase', 50);
            $table->string('phraseLower', 50);
            $table->string('basePhrase', 50);
            $table->string('definition', 500);
            $table->integer('text_id')->nullable()->unsigned();
            $table->integer('language_id')->unsigned();
            $table->timestamp('created_at');
            $table->timestamp('updated_at');
            $table->integer('user_id')->nullable()->unsigned();
            
            $table->foreign('language_id')->references('id')->on('languages');
            $table->foreign('user_id')->references('id')->on('users');
            $table->index('state');
            $table->index('phraseLower');
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        Schema::drop('terms');
    }

}
